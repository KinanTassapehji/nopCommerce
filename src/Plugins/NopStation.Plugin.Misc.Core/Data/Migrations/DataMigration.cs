using System;
using System.Collections.Generic;
using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Data.Migrations;

[NopMigration("2026-02-09 00:00:00", "NopStation.Core SMS template create", MigrationProcessType.NoMatter)]
public class DataMigration : ForwardOnlyMigration
{
	private readonly INopDataProvider _dataProvider;

	public DataMigration(INopDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
	}

	public override void Up()
	{
		List<SmsTemplate> entities = new List<SmsTemplate>
		{
			new SmsTemplate
			{
				Name = "Customer.EmailValidationMessage",
				Body = $"%Store.Name%, {Environment.NewLine}Check your email to activate your account. {Environment.NewLine}%Store.Name%",
				Active = true
			},
			new SmsTemplate
			{
				Name = "Customer.NewPM",
				Body = "%Store.Name%, " + Environment.NewLine + "You have received a new private message.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "Customer.WelcomeMessage",
				Body = $"We welcome you to %Store.Name%.{Environment.NewLine}You can now take part in the various services we have to offer you. Some of these services include:{Environment.NewLine}Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.{Environment.NewLine}Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.{Environment.NewLine}Order History - View your history of purchases that you have made with us.{Environment.NewLine}Products Reviews - Share your opinions on products with our other customers.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "Forums.NewForumPost",
				Body = $"%Store.Name%, {Environment.NewLine}A new post has been created in the topic %Forums.TopicName% at %Forums.ForumName% forum.{Environment.NewLine}Click here for more info.{Environment.NewLine}Post author: %Forums.PostAuthor%{Environment.NewLine}Post body: %Forums.PostBody%",
				Active = true
			},
			new SmsTemplate
			{
				Name = "Forums.NewForumTopic",
				Body = $"%Store.Name%, {Environment.NewLine}A new topic %Forums.TopicName% has been created at %Forums.ForumName% forum.{Environment.NewLine}Click here for more info.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "NewCustomer.Notification",
				Body = $"%Store.Name%, {Environment.NewLine}A new customer registered with your store. Below are the customer's details:{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}Email: %Customer.Email%.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "OrderCancelled.CustomerNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been cancelled. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "OrderCompleted.CustomerNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Your order has been completed. Below is the summary of the order.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "ShipmentDelivered.CustomerNotification",
				Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Good news! You order has been delivered.{Environment.NewLine}Order Number: %Order.OrderNumber%.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "ShipmentDelivered.CustomerOTPNotification",
				Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Here is your OTP code: %Shipment.OTP%",
				Active = true
			},
			new SmsTemplate
			{
				Name = "OrderPlaced.CustomerNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order Number: %Order.OrderNumber%.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "OrderPlaced.AdminNotification",
				Body = "%Store.Name%, " + Environment.NewLine + "%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store.",
				Active = true
			},
			new SmsTemplate
			{
				Name = "ShipmentSent.CustomerNotification",
				Body = $" %Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}Good news! You order has been shipped.{Environment.NewLine}Order Number: %Order.OrderNumber%",
				Active = true
			},
			new SmsTemplate
			{
				Name = "OrderPlaced.VendorNotification",
				Body = $"%Store.Name%, {Environment.NewLine}%Customer.FullName% (%Customer.Email%) has just placed an order.{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}.",
				Active = false
			},
			new SmsTemplate
			{
				Name = "OrderRefunded.CustomerNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been has been refunded. Please allow 7-14 days for the refund to be reflected in your account.",
				Active = false
			},
			new SmsTemplate
			{
				Name = "OrderRefunded.AdminNotification",
				Body = $"%Store.Name%. Order #%Order.OrderNumber% refunded', %Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just refunded{Environment.NewLine}Amount refunded: %Order.AmountRefunded%{Environment.NewLine}Date Ordered: %Order.CreatedOn%.",
				Active = false
			},
			new SmsTemplate
			{
				Name = "OrderPaid.AdminNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just paid{Environment.NewLine}Date Ordered: %Order.CreatedOn%.",
				Active = false
			},
			new SmsTemplate
			{
				Name = "OrderPaid.CustomerNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}Thanks for buying from %Store.Name%. Order #%Order.OrderNumber% has been just paid. Order Number: %Order.OrderNumber%.",
				Active = false
			},
			new SmsTemplate
			{
				Name = "OrderPaid.VendorNotification",
				Body = $"%Store.Name%, {Environment.NewLine}Order #%Order.OrderNumber% has been just paid.{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}.",
				Active = false
			}
		};
		_dataProvider.BulkInsertEntities(entities);
	}
}
