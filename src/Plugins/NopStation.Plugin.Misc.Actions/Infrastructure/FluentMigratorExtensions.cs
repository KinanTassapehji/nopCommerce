using System;
using System.Data;
using System.Linq.Expressions;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Schema.Column;
using FluentMigrator.Builders.Schema.Index;
using FluentMigrator.Builders.Schema.Table;
using Nop.Core;
using Nop.Data.Extensions;
using Nop.Data.Mapping;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public static class FluentMigratorExtensions
{
	public static ISchemaTableSyntax Table<TEntity>(this ISchemaExpressionRoot schema) where TEntity : BaseEntity
	{
		return schema.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax Table<TEntity>(this IAlterExpressionRoot root) where TEntity : BaseEntity
	{
		return root.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static void TableFor<TEntity>(this ICreateExpressionRoot expressionRoot, ISchemaExpressionRoot schema, bool checkIfExists = true) where TEntity : BaseEntity
	{
		Type typeFromHandle = typeof(TEntity);
		string tableName = NameCompatibilityManager.GetTableName(typeFromHandle);
		if (!checkIfExists || !schema.Table(tableName).Exists())
		{
			(expressionRoot.Table(tableName) as CreateTableExpressionBuilder).RetrieveTableExpressions(typeFromHandle);
		}
	}

	public static void To<TEntity>(this IRenameTableToOrInSchemaSyntax table) where TEntity : BaseEntity
	{
		table.To(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static ISchemaColumnSyntax TableColumn<TEntity>(this ISchemaExpressionRoot schema, string columnName) where TEntity : BaseEntity
	{
		columnName = NameCompatibilityManager.GetColumnName(typeof(TEntity), columnName);
		return schema.Table<TEntity>().Column(columnName);
	}

	public static ISchemaColumnSyntax TableColumn<TEntity>(this ISchemaExpressionRoot schema, Expression<Func<TEntity, object>> selector) where TEntity : BaseEntity
	{
		string columnName = GetColumnName(selector);
		columnName = NameCompatibilityManager.GetColumnName(typeof(TEntity), columnName);
		return schema.Table<TEntity>().Column(columnName);
	}

	public static IRenameColumnToOrInSchemaSyntax TableColumn<TEntity>(this IRenameExpressionRoot root, string columnName) where TEntity : BaseEntity
	{
		return root.Column(columnName).OnTable(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static void TableColumnTo<TEntity>(this IRenameExpressionRoot root, Expression<Func<TEntity, object>> selector, string oldColumnName) where TEntity : BaseEntity
	{
		string columnName = GetColumnName(selector);
		columnName = NameCompatibilityManager.GetColumnName(typeof(TEntity), columnName);
		root.Column(oldColumnName).OnTable(NameCompatibilityManager.GetTableName(typeof(TEntity))).To(columnName);
	}

	public static IAlterTableColumnAsTypeSyntax AddTableColumn<TEntity>(this IAlterExpressionRoot root, string columnName) where TEntity : BaseEntity
	{
		columnName = NameCompatibilityManager.GetColumnName(typeof(TEntity), columnName);
		return root.Table<TEntity>().AddColumn(columnName);
	}

	public static IAlterTableColumnAsTypeSyntax AddTableColumn<TEntity>(this IAlterExpressionRoot root, Expression<Func<TEntity, object>> selector) where TEntity : BaseEntity
	{
		string columnName = GetColumnName(selector);
		columnName = NameCompatibilityManager.GetColumnName(typeof(TEntity), columnName);
		return root.Table<TEntity>().AddColumn(columnName);
	}

	public static ISchemaColumnSyntax AlterTableColumn<TEntity>(this ISchemaExpressionRoot schema, string columnName) where TEntity : BaseEntity
	{
		columnName = NameCompatibilityManager.GetColumnName(typeof(TEntity), columnName);
		return schema.Table<TEntity>().Column(columnName);
	}

	private static string GetColumnName<TEntity>(Expression<Func<TEntity, object>> selector) where TEntity : BaseEntity
	{
		MemberExpression memberExpression = selector.Body as MemberExpression;
		if (memberExpression == null)
		{
			memberExpression = (selector.Body as UnaryExpression)?.Operand as MemberExpression;
		}
		return memberExpression?.Member.Name;
	}

	public static ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey<TPrimary>(this ICreateTableColumnOptionOrWithColumnSyntax column, string primaryTableName = null, string primaryColumnName = null, Rule onDelete = Rule.Cascade) where TPrimary : BaseEntity
	{
		if (string.IsNullOrEmpty(primaryTableName))
		{
			primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));
		}
		if (string.IsNullOrEmpty(primaryColumnName))
		{
			primaryColumnName = "Id";
		}
		return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
	}

	public static IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey<TPrimary>(this IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax column, string primaryTableName = null, string primaryColumnName = null, Rule onDelete = Rule.Cascade) where TPrimary : BaseEntity
	{
		if (string.IsNullOrEmpty(primaryTableName))
		{
			primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));
		}
		if (string.IsNullOrEmpty(primaryColumnName))
		{
			primaryColumnName = "Id";
		}
		return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
	}

	public static IIfExistsOrInSchemaSyntax Table<TEntity>(this IDeleteExpressionRoot syntax) where TEntity : BaseEntity
	{
		return syntax.Table(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static IDeleteIndexOnColumnOrInSchemaSyntax IndexOnTable<TEntity>(this IDeleteExpressionRoot root, string indexName) where TEntity : BaseEntity
	{
		return root.Index(indexName).OnTable(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static ICreateIndexOnColumnOrInSchemaSyntax IndexOnTable<TEntity>(this ICreateExpressionRoot root, string indexName) where TEntity : BaseEntity
	{
		return root.Index(indexName).OnTable(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static ICreateIndexOnColumnOrInSchemaSyntax OnTable<TEntity>(this ICreateIndexForTableSyntax tableSyntax) where TEntity : BaseEntity
	{
		return tableSyntax.OnTable(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static IDeleteIndexOnColumnOrInSchemaSyntax OnTable<TEntity>(this IDeleteIndexForTableSyntax tableSyntax) where TEntity : BaseEntity
	{
		return tableSyntax.OnTable(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static ISchemaIndexSyntax TableIndex<TEntity>(this ISchemaExpressionRoot schema, string indexName) where TEntity : BaseEntity
	{
		return schema.Table(NameCompatibilityManager.GetTableName(typeof(TEntity))).Index(indexName);
	}

	public static IInSchemaSyntax ColumnFromTable<TEntity>(this IDeleteExpressionRoot root, string columnName) where TEntity : BaseEntity
	{
		return root.Column(columnName).FromTable(NameCompatibilityManager.GetTableName(typeof(TEntity)));
	}

	public static (Type propType, bool canBeNullable) GetTypeToMap(this Type type)
	{
		Type underlyingType = Nullable.GetUnderlyingType(type);
		if ((object)underlyingType != null)
		{
			return (propType: underlyingType, canBeNullable: true);
		}
		return (propType: type, canBeNullable: false);
	}
}
