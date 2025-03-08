using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Constants
{
    /// <summary>
    /// Constants for resource keys
    /// </summary>
    public static class ResourceKeys
    {
        // Common error messages
        public static class Errors
        {
            public const string UserNotFound = "UserNotFound";
            public const string ProductNotFound = "ProductNotFound";
            public const string CategoryNotFound = "CategoryNotFound";
            public const string CompanyNotFound = "CompanyNotFound";
            public const string UserNotBelongToCompany = "UserNotBelongToCompany";
            public const string CategoryNameAlreadyExists = "CategoryNameAlreadyExists";
            public const string InvalidVerificationCode = "InvalidVerificationCode";
            public const string InvalidCredentials = "InvalidCredentials";
            public const string ProductNameAlreadyExists = "ProductNameAlreadyExists";
            public const string SkuAlreadyExists = "SkuAlreadyExists";
            public const string BarcodeAlreadyExists = "BarcodeAlreadyExists";
            public const string EmailAlreadyExists = "EmailAlreadyExists";
            public const string PhoneNumberAlreadyExists = "PhoneNumberAlreadyExists";
            public const string TaxNumberAlreadyExists = "TaxNumberAlreadyExists";
            public const string OrderPlacementError = "OrderPlacementError";
        }

        // Validation messages
        public static class Validation
        {
            public const string RequiredField = "RequiredField";
            public const string InvalidEmail = "InvalidEmail";
            public const string InvalidPassword = "InvalidPassword";
            public const string InvalidPhoneNumber = "InvalidPhoneNumber";
            public const string InvalidTaxNumber = "InvalidTaxNumber";
            
            // Product validation
            public const string ProductNameRequired = "ProductNameRequired";
            public const string ProductNameMaxLength = "ProductNameMaxLength";
            public const string ProductSkuRequired = "ProductSkuRequired";
            public const string ProductSkuMaxLength = "ProductSkuMaxLength";
            public const string ProductDescriptionMaxLength = "ProductDescriptionMaxLength";
            public const string ProductPriceGreaterThanZero = "ProductPriceGreaterThanZero";
            public const string ProductBarcodeMaxLength = "ProductBarcodeMaxLength";
            
            // Category validation
            public const string CategoryNameRequired = "CategoryNameRequired";
            public const string CategoryNameMaxLength = "CategoryNameMaxLength";
            public const string CategoryDescriptionMaxLength = "CategoryDescriptionMaxLength";
            
            // User validation
            public const string UserNameRequired = "UserNameRequired";
            public const string UserNameMaxLength = "UserNameMaxLength";
            public const string UserEmailRequired = "UserEmailRequired";
            public const string UserEmailMaxLength = "UserEmailMaxLength";
            public const string UserPasswordRequired = "UserPasswordRequired";
            public const string UserPasswordMinLength = "UserPasswordMinLength";
            public const string UserPhoneNumberMaxLength = "UserPhoneNumberMaxLength";
            
            // Customer validation
            public const string TcknRequired = "TcknRequired";
            public const string TcknLength = "TcknLength";
            public const string FirstNameRequired = "FirstNameRequired";
            public const string LastNameRequired = "LastNameRequired";
            public const string PhoneNumberRequired = "PhoneNumberRequired";
            public const string PhoneNumberLength = "PhoneNumberLength";
            public const string AddressRequired = "AddressRequired";
            public const string CityRequired = "CityRequired";
            
            // Company validation
            public const string CompanyNameRequired = "CompanyNameRequired";
            public const string CompanyNameMaxLength = "CompanyNameMaxLength";
            public const string CompanyTaxNumberRequired = "CompanyTaxNumberRequired";
            public const string CompanyTaxNumberMaxLength = "CompanyTaxNumberMaxLength";
            public const string CompanyAddressMaxLength = "CompanyAddressMaxLength";
            public const string CompanyPhoneNumberMaxLength = "CompanyPhoneNumberMaxLength";
            
            // Order validation
            public const string OrderItemsRequired = "OrderItemsRequired";
            public const string PaymentsRequired = "PaymentsRequired";
            public const string ProductRequired = "ProductRequired";
            public const string QuantityGreaterThanZero = "QuantityGreaterThanZero";
            public const string TotalAmountNotNegative = "TotalAmountNotNegative";
            public const string PaymentAmountGreaterThanZero = "PaymentAmountGreaterThanZero";
            public const string PaymentMethodRequired = "PaymentMethodRequired";
            public const string InvalidPaymentMethod = "InvalidPaymentMethod";
            
            // Additional validation
            public const string RolesRequired = "RolesRequired";
            public const string AtLeastOneRoleRequired = "AtLeastOneRoleRequired";
            public const string WebsiteMaxLength = "WebsiteMaxLength";
            public const string DescriptionMaxLength = "DescriptionMaxLength";
            
            // Pagination validation
            public const string PageNumberGreaterThanZero = "PageNumberGreaterThanZero";
            public const string PageSizeGreaterThanZero = "PageSizeGreaterThanZero";
            
            // Pending user validation
            public const string FirstNameMaxLength = "FirstNameMaxLength";
            public const string LastNameMaxLength = "LastNameMaxLength";
            public const string CityMaxLength = "CityMaxLength";
            
            // Auth validation
            public const string OtpRequired = "OtpRequired";
        }

        // Success messages
        public static class Success
        {
            public const string OperationSuccessful = "OperationSuccessful";
            public const string RecordCreated = "RecordCreated";
            public const string RecordUpdated = "RecordUpdated";
            public const string RecordDeleted = "RecordDeleted";
        }
    }
} 