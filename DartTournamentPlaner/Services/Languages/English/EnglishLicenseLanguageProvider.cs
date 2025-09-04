using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for license system
/// </summary>
public class EnglishLicenseLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // License Menu
            ["License"] = "License",
            ["LicenseStatus"] = "License Status",
            ["LicenseStatusSubtitle"] = "Overview of your current license status and available features",
            ["ActivateLicense"] = "Activate License",
            ["LicenseInfo"] = "License Information",
            ["LicenseInfoSubtitle"] = "Current license and feature information",
            ["PurchaseLicense"] = "Get your License",
            ["ShowDetails"] = "Show Details",
            ["ActivatingLicense"] = "Activating license...",
            ["ValidatingLicense"] = "Validating license...",
            ["LicenseActivationMessage"] = "Enter your license key:",
            ["LicenseActivatedSuccessfully"] = "License activated successfully!",
            ["PurchaseLicenseMessage"] = "Visit our website to purchase a license:",
            ["RemoveLicense"] = "Remove License",
            ["RemoveLicenseConfirmation"] = "Do you really want to remove the activated license?",
            ["LicenseRemovedSuccess"] = "License has been successfully removed!",
            ["LicenseRemoveError"] = "Error removing license.",
            ["Success"] = "Success",
            ["Refresh"] = "Refresh",

            // Extended License Status Translations
            ["LicenseValid"] = "License valid",
            ["LicenseInvalid"] = "License invalid",
            ["LicenseExpired"] = "License expired",
            ["LicenseActivationRequired"] = "License activation required",
            ["LicensedMode"] = "Licensed mode",
            ["UnlicensedMode"] = "Unlicensed mode",
            ["OfflineMode"] = "Offline mode",
            ["OnlineMode"] = "Online mode",

            // Purchase License Dialog Translations
            ["PurchaseLicenseTitle"] = "Get your License",
            ["PurchaseLicenseSubtitle"] = "Fill out the form to request a license. \n At the beginning License will be free for personal use. ",
            ["PurchasePersonalInformation"] = "Personal Information",
            ["PurchaseLicenseRequirements"] = "License Requirements",
            ["PurchaseAdditionalInformation"] = "Additional Information",
            ["PurchaseSystemInformation"] = "System Information",
            ["PurchaseHardwareInfoII"] = "This Hardware ID will be used for license activation and is required for processing your request.",
            ["PurchaseHardwareInfoI"] = "Your Hardware ID (automatically included in the request):",
            ["FirstName"] = "First Name",
            ["LastName"] = "Last Name",
            ["Email"] = "Email Address",
            ["Company"] = "Company / Organization",
            ["LicenseType"] = "License Type",
            ["RequiredActivations"] = "Required Activations",
            ["RequiredFeatures"] = "Required Features",
            ["AdditionalMessage"] = "Message / Special Requirements",
            ["HowDidYouHear"] = "How did you hear about us?",
            ["SendLicenseRequest"] = "Send License Request",
            ["ValidationError"] = "Validation Error",
            ["EmailClientOpened"] = "Email client opened",

            // Donation Dialog
            ["SupportProject"] = "Support Project",
            ["SupportProjectTitle"] = "Support Our Project",
            ["SupportProjectSubtitle"] = "Your donation helps us develop Dart Tournament Planner further!",
            ["SelectDonationAmount"] = "Select your donation amount:",
            ["Skip"] = "Skip",
            ["DonateViaPayPal"] = "Donate via PayPal",
            ["ThankYou"] = "Thank you!",
            ["PayPalOpened"] = "PayPal opened for donation",

            // DONATION DIALOG - NEW
            ["SupportDevelopmentTitle"] = "Support Development",
            ["DonationDialogMessage"] = "Do you like this Dart Tournament Planner?\n\nSupport further development with a small donation.\nEvery contribution helps improve and maintain the software.",
            ["WithYourSupportYouEnable"] = "With your support you enable:",
            ["RegularUpdatesAndNewFeatures"] = "• Regular updates and new features",
            ["FastBugFixes"] = "• Fast bug fixes",
            ["ContinuousSupport"] = "• Continuous support",
            ["FreeUseForEveryone"] = "• Free use for everyone",
            ["OpenDonationPageButton"] = "Open Donation Page",
            ["LaterButton"] = "Later",

            // TOURNAMENT OVERVIEW LICENSE DIALOG - NEW
            ["TournamentOverviewLicenseRequiredTitle"] = "Tournament Overview License Required",
            ["TournamentOverviewLicenseRequiredSubtitle"] = "Premium feature for professional tournament management",
            ["TournamentOverviewLicenseMessage"] = "The advanced tournament overview is a premium feature that requires a valid license.",
            ["TournamentOverviewBenefitsTitle"] = "Tournament overview provides:",
            ["TournamentOverviewBenefits"] = "• Live display of all tournament classes\n• Automatic switching between phases\n• Professional presentation for spectators\n• Real-time updates and synchronization\n• Customizable display options",
            ["TournamentOverviewActionText"] = "Would you like to request a license with tournament overview features?",
            ["RequestTournamentOverviewLicense"] = "Request Tournament Overview License",

            // PRINT LICENSE
            ["PrintLicenseRequired"] = "Print license required",
            ["PrintLicenseRequiredTitle"] = "Print License Required",
            ["PrintLicenseRequiredMessage"] = "Print functionality requires a valid license with the 'Enhanced Printing' feature.",
            ["EnhancedPrintingBenefitsTitle"] = "Enhanced Printing includes:",
            ["EnhancedPrintingBenefits"] = "- Professional tournament reports\n- Match results and tournament trees\n- Export to PDF functionality",
            ["PrintLicenseActionText"] = "Would you like to request a license with the Enhanced Printing feature?",
            ["RequestLicense"] = "Get your License",
            ["RequestPrintLicense"] = "Request Print License",

            // Statistics License Messages
            ["StatisticsLicenseRequiredTitle"] = "Statistics License Required",
            ["StatisticsLicenseRequiredMessage"] = "Advanced statistics require a valid license with the 'Statistics' feature and an active Hub connection.",
            ["StatisticsBenefitsTitle"] = "Statistics features include:",
            ["StatisticsBenefits"] = "- Detailed player performance analysis\n- Match history and trends\n- Tournament progress tracking\n- Real-time Hub synchronization\n- Advanced statistical reports",
            ["HubConnectionRequired"] = "Hub connection required",
            ["HubConnectionRequiredText"] = "Statistics features require an active Tournament Hub connection for real-time data synchronization.",
            ["StatisticsLicenseActionText"] = "Would you like to request a license with Statistics and Hub features?",
            ["RequestStatisticsLicense"] = "Get your License",

            // API License Texts
            ["ApiLicenseRequired"] = "API license required",
            ["ApiLicenseRequiredTitle"] = "API License Required",
            ["ApiLicenseRequiredMessage"] = "API functionality requires a valid license with the 'API Connection' feature.",
            ["ApiConnectionBenefitsTitle"] = "API Connection features include:",
            ["ApiConnectionBenefits"] = "- REST API for tournament data\n- Webhooks for real-time updates\n- External integrations\n- Automated data processing",
            ["ApiLicenseActionText"] = "Would you like to request a license with API Connection features?",
            ["RequestApiLicense"] = "Get your License",

            // License Activation Dialog
            ["LicenseKeyFormatInfo"] = "License keys consist of 8 blocks with 4 characters each (A-F, 0-9), separated by hyphens.",
            ["InvalidLicenseKeyFormat"] = "Invalid license key format. Please check your input.",
            ["Continue"] = "Continue",
            ["Unlimited"] = "Unlimited",

            // License Success Dialog
            ["LicenseActivationSuccessSubtitle"] = "All premium features are now available",
            ["NoSpecificFeatures"] = "All standard features available",
            ["GenericLicenseActivated"] = "Your license has been successfully activated.",
            ["LastActivationWarning"] = "This was your last available activation for this license. Contact support if you need to install the software on additional computers.",
            ["FewActivationsWarning"] = "You have {0} activation(s) remaining for this license. Plan additional installations carefully."
        };
    }
}