/*
    Removes every trace of the deleted NopStation.Plugin.Widgets.SmartTabs plugin
    from the TmTm database. Idempotent - safe to run more than once, and safe on a
    database where the plugin was never installed.

        sqlcmd -S . -d TmTm -E -i drop-smarttabs.sql

    (-E is Windows integrated auth. For SQL auth, pass the credentials from the
    environment rather than writing them here:
        sqlcmd -S . -d TmTm -U "$SQL_USER" -P "$SQL_PASSWORD" -i drop-smarttabs.sql)

    Run it with the store STOPPED.
*/

SET NOCOUNT ON;
USE [TmTm];
GO

/* --- 1. Tables ---------------------------------------------------------------
   Dropped children-first: the mapping tables carry FKs to NS_SmartTabs_SmartTab
   and NS_SmartTabs_SmartTabGroup. */
DECLARE @tables TABLE (name sysname, ord int);
INSERT INTO @tables (name, ord) VALUES
    ('NS_SmartTabs_AnalyticsEvent',                1),
    ('NS_SmartTabs_SmartTabProductMapping',        1),
    ('NS_SmartTabs_SmartTabCategoryMapping',       1),
    ('NS_SmartTabs_SmartTabManufacturerMapping',   1),
    ('NS_SmartTabs_SmartTabCustomerRoleMapping',   1),
    ('NS_SmartTabs_SmartTabTopicMapping',          1),
    ('NS_SmartTabs_SmartTabVendorMapping',         1),
    ('NS_SmartTabs_SmartTabBlogPostMapping',       1),
    ('NS_SmartTabs_SmartTabNewsItemMapping',       1),
    ('NS_SmartTabs_SmartTabRevision',              1),
    ('NS_SmartTabs_SmartTabRule',                  1),
    ('NS_SmartTabs_SmartTabExperiment',            1),
    ('NS_SmartTabs_SmartTabTemplate',              2),
    ('NS_SmartTabs_SmartTab',                      3),
    ('NS_SmartTabs_SmartTabGroup',                 4);

DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'DROP TABLE [dbo].' + QUOTENAME(name) + N';' + CHAR(10)
FROM (SELECT name, ord FROM @tables) t
WHERE OBJECT_ID(N'[dbo].' + QUOTENAME(name), N'U') IS NOT NULL
ORDER BY ord, name;

IF @sql <> N'' EXEC sp_executesql @sql;
PRINT 'tables dropped';
GO

/* --- 2. Settings -------------------------------------------------------------
   nopCommerce stores ISettings property names lowercased as
   "smarttabssettings.<property>". Also drop the plugin from the active widget
   list, which is a comma-separated string in widgetsettings.activewidgetsystemnames. */
DELETE FROM [Setting] WHERE [Name] LIKE 'smarttabssettings.%';

UPDATE [Setting]
SET [Value] = NULLIF(
        /* rebuild the CSV without our system name, then squeeze the double comma */
        REPLACE(
            REPLACE(',' + REPLACE([Value], ' ', '') + ',',
                    ',NopStation.Plugin.Widgets.SmartTabs,', ','),
            ',,', ','),
        ',')
WHERE [Name] = 'widgetsettings.activewidgetsystemnames'
  AND [Value] LIKE '%NopStation.Plugin.Widgets.SmartTabs%';

/* the rebuild above leaves a leading and trailing comma on a non-empty list */
UPDATE [Setting]
SET [Value] = SUBSTRING([Value], 2, LEN([Value]) - 2)
WHERE [Name] = 'widgetsettings.activewidgetsystemnames'
  AND LEFT([Value], 1) = ',' AND RIGHT([Value], 1) = ',';
PRINT 'settings removed';
GO

/* --- 3. Locale resources ------------------------------------------------------ */
DELETE FROM [LocaleStringResource]
WHERE [ResourceName] LIKE 'Plugins.NopStation.SmartTabs.%'
   OR [ResourceName] LIKE 'Admin.NopStation.SmartTabs.%'
   OR [ResourceName] LIKE 'Enums.NopStation.Plugin.Widgets.SmartTabs.%'
   /* the display name nopCommerce generates for the plugin's permission */
   OR [ResourceName] = 'security.permission.managesmarttabs';
PRINT 'locale resources removed';
GO

/* --- 4. Permission ------------------------------------------------------------
   The mapping table has an FK to PermissionRecord, so it goes first. nopCommerce
   maps it to PermissionRecord_Role_Mapping via BaseNameCompatibility. */
DELETE m
FROM [PermissionRecord_Role_Mapping] m
JOIN [PermissionRecord] p ON p.[Id] = m.[PermissionRecord_Id]
WHERE p.[SystemName] = 'ManageSmartTabs';

DELETE FROM [PermissionRecord] WHERE [SystemName] = 'ManageSmartTabs';
PRINT 'permission removed';
GO

/* --- 5. Migration version records ---------------------------------------------
   FluentMigrator remembers every migration it has run. Left behind, these would
   block a reinstall of a plugin with the same migration ids. */
IF OBJECT_ID(N'[dbo].[MigrationVersionInfo]', N'U') IS NOT NULL
    DELETE FROM [MigrationVersionInfo] WHERE [Description] LIKE '%SmartTab%';
PRINT 'migration records removed';
GO

PRINT 'SmartTabs removed from TmTm.';
GO
