using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Erp.Domain.DTOs.Order;
using Erp.Domain.DTOs.Order.Report;
using Erp.Domain.Entities;
using Erp.Domain.Enums;

namespace Erp.Application.Mappings;

public class OrderProfile : Profile
{
	public OrderProfile()
	{
		CreateMap<PlaceOrderItemDto, OrderItem>();
		CreateMap<PlaceOrderPaymentDto, OrderPayment>();
		CreateMap<PlaceOrderPaymentDto, Discount>();
		CreateMap<PlaceOrderDto, Order>()
		.ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
		.ForMember(dest => dest.OrderPayments, opt => opt.MapFrom(src => src.Payments))
		.ForMember(dest => dest.Discounts, opt =>
		 opt.MapFrom(src =>
		  src.Payments.Where(x => x.PaymentMethod == PaymentMethods.Discount).ToList()))
		  .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Payments.Sum(x => x.Amount)))
		  .ForMember(dest => dest.DiscountAmount, opt =>
		   opt.MapFrom(src => src.Payments.Where(x => x.PaymentMethod == PaymentMethods.Discount).Sum(x => x.Amount)))
		  .ForMember(dest => dest.NetAmount, opt =>
		   opt.MapFrom(src => src.Payments.Sum(x => x.Amount) - src.Payments.Where(x => x.PaymentMethod == PaymentMethods.Discount).Sum(x => x.Amount)))
		  .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.OrderItems.Sum(o => o.Quantity)));

		// Map from entities to DTOs
		CreateMap<Order, OrderDto>();
		CreateMap<OrderItem, OrderItemDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
		CreateMap<OrderPayment, OrderPaymentDto>();
		CreateMap<Order, OrderHistoryDto>();
		CreateMap<Order, OrderHistoryExcelDto>()
			.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd/MM/yyyy")))
			.ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedAt.ToString("HH:mm")));
	}
}