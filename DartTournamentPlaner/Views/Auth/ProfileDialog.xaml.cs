using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Views.Common;

namespace DartTournamentPlaner.Views.Auth;

public partial class ProfileDialog : Window
{
    private readonly UserAuthService _authService;
    private readonly LocalizationService _localization;
    private readonly LicenseManager _licenseManager;

    public ProfileDialog(UserAuthService authService, LocalizationService localization, LicenseManager licenseManager)
    {
        _authService = authService;
        _localization = localization;
        _licenseManager = licenseManager;
        InitializeComponent();
        ApplyTranslations();
        LoadUser();
    }

    private void ApplyTranslations()
    {
        TitleBlock.Text = _localization.GetString("Profile") ?? "Profile";
        InfoText.Text = _localization.GetString("ProfileInfoText") ?? "Profilinformationen werden angezeigt.";
        UsernameLabel.Text = _localization.GetString("Username") ?? "Username";
        NameLabel.Text = _localization.GetString("Name") ?? "Name";
        EmailLabel.Text = _localization.GetString("Email") ?? "Email";
        LicenseLabel.Text = _localization.GetString("LicenseKey") ?? "License";
        CloseButton.Content = _localization.GetString("Cancel") ?? "Cancel";
        SaveButton.Content = _localization.GetString("Save") ?? "Save";
        FillLicenseButton.ToolTip = _localization.GetString("LicenseAutoFill") ?? "Lizenz aus Anwendung übernehmen";
    }

    private void LoadUser()
    {
        var user = _authService.CurrentUser;
        if (user == null)
        {
            StatusMessage.Text = _localization.GetString("AuthNotLoggedIn") ?? "Not logged in.";
            SaveButton.IsEnabled = false;
            return;
        }

        UsernameValue.Text = user.Username;
        FirstNameValue.Text = user.Vorname;
        LastNameValue.Text = user.Name;
        EmailValue.Text = user.Email;
        LicenseValue.Text = user.LicenseKey ?? string.Empty;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;
        StatusMessage.Text = string.Empty;

        var name = LastNameValue.Text.Trim();
        var vorname = FirstNameValue.Text.Trim();
        var email = EmailValue.Text.Trim();
        var licenseKey = string.IsNullOrWhiteSpace(LicenseValue.Text) ? null : LicenseValue.Text.Trim();

        var result = await _authService.UpdateProfileAsync(name, vorname, email, licenseKey);
        if (result.Success)
        {
            StyledInfoDialog.Show(
                _localization.GetString("Success") ?? "Success",
                result.Message ?? (_localization.GetString("ProfileUpdated") ?? "Profile updated."),
                _localization,
                isSuccess: true,
                owner: this);
            DialogResult = true;
            Close();
        }
        else
        {
            StatusMessage.Text = result.Message ?? (_localization.GetString("Error") ?? "Error");
        }

        SaveButton.IsEnabled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void FillLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        var license = _licenseManager.GetStoredLicenseKey();
        if (!string.IsNullOrWhiteSpace(license))
        {
            LicenseValue.Text = license;
            StatusMessage.Text = _localization.GetString("LicenseAutoFillSuccess") ?? "Lizenzschlüssel übernommen.";
        }
        else
        {
            StatusMessage.Text = _localization.GetString("LicenseAutoFillMissing") ?? "Kein Lizenzschlüssel gefunden.";
        }
    }
}
