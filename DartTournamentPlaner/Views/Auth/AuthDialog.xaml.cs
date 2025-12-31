using System;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views.Common;

namespace DartTournamentPlaner.Views.Auth;

public partial class AuthDialog : Window
{
    private readonly UserAuthService _authService;
    private readonly LocalizationService _localization;
    private bool _isBusy;

    public AuthDialog(UserAuthService authService, LocalizationService localization)
    {
        _authService = authService;
        _localization = localization;

        InitializeComponent();
        UpdateTranslations();

        var lastUser = App.ConfigService?.Config.AuthUsername;
        LoginUsernameTextBox.Text = _authService.CurrentUser?.Username ?? lastUser ?? string.Empty;
        RememberSessionCheckBox.IsChecked = App.ConfigService?.Config.RememberAuthSession ?? false;
    }

    private void UpdateTranslations()
    {
        Title = _localization.GetString("Account") ?? "Account";
        HeaderTitle.Text = _localization.GetString("Account") ?? "Account";

        LoginSubtitle.Text = _localization.GetString("LoginSubtitle") ?? "Sign in with your account.";
        LoginUsernameLabel.Text = _localization.GetString("Username") ?? "Username";
        LoginPasswordLabel.Text = _localization.GetString("Password") ?? "Password";
        RememberSessionCheckBox.Content = _localization.GetString("RememberSession") ?? "Remember me";
        LoginButton.Content = _localization.GetString("Login") ?? "Login";
        CancelLoginButton.Content = _localization.GetString("Cancel") ?? "Cancel";

        RegisterSubtitle.Text = _localization.GetString("RegisterSubtitle") ?? "Create a new account.";
        RegisterUsernameLabel.Text = _localization.GetString("Username") ?? "Username";
        RegisterEmailLabel.Text = _localization.GetString("Email") ?? "Email";
        RegisterPasswordLabel.Text = _localization.GetString("Password") ?? "Password";
        RegisterPasswordRepeatLabel.Text = _localization.GetString("ConfirmPassword") ?? "Repeat Password";
        RegisterFirstNameLabel.Text = _localization.GetString("FirstName") ?? "First name";
        RegisterLastNameLabel.Text = _localization.GetString("LastName") ?? "Last name";
        RegisterLicenseLabel.Text = _localization.GetString("LicenseKey") ?? "License key (optional)";
        RegisterButton.Content = _localization.GetString("Register") ?? "Register";
        CancelRegisterButton.Content = _localization.GetString("Cancel") ?? "Cancel";
    }

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        SetButtonsEnabled(false);

        try
        {
            var request = new UserLoginRequest
            {
                Username = LoginUsernameTextBox.Text.Trim(),
                Password = LoginPasswordBox.Password,
                RememberSession = RememberSessionCheckBox.IsChecked == true
            };

            var result = await _authService.LoginAsync(request);
            if (result.Success)
            {
                StyledInfoDialog.Show(
                    _localization.GetString("Success") ?? "Success",
                    result.Message ?? (_localization.GetString("LoginSuccess") ?? "Login successful."),
                    _localization,
                    isSuccess: true,
                    owner: this);
                DialogResult = true;
                Close();
            }
            else
            {
                LoginMessageBlock.Text = result.Message ?? (_localization.GetString("AuthInvalidCredentials") ?? "Invalid username or password.");
            }
        }
        catch (Exception ex)
        {
            LoginMessageBlock.Text = ex.Message;
        }
        finally
        {
            _isBusy = false;
            SetButtonsEnabled(true);
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;
        _isBusy = true;
        SetButtonsEnabled(false);

        try
        {
            var request = new UserRegistrationRequest
            {
                Username = RegisterUsernameTextBox.Text.Trim(),
                Password = RegisterPasswordBox.Password,
                PasswordRepeat = RegisterPasswordRepeatBox.Password,
                Email = RegisterEmailTextBox.Text.Trim(),
                Name = RegisterLastNameTextBox.Text.Trim(),
                Vorname = RegisterFirstNameTextBox.Text.Trim(),
                LicenseKey = string.IsNullOrWhiteSpace(RegisterLicenseTextBox.Text) ? null : RegisterLicenseTextBox.Text.Trim()
            };

            var result = await _authService.RegisterAsync(request);
            if (result.Success)
            {
                StyledInfoDialog.Show(
                    _localization.GetString("Success") ?? "Success",
                    result.Message ?? (_localization.GetString("RegistrationSuccess") ?? "Registration successful."),
                    _localization,
                    isSuccess: true,
                    owner: this);
                DialogResult = true;
                Close();
            }
            else
            {
                RegisterMessageBlock.Text = result.Message ?? (_localization.GetString("AuthRegistrationFailed") ?? "Registration failed.");
            }
        }
        catch (Exception ex)
        {
            RegisterMessageBlock.Text = ex.Message;
        }
        finally
        {
            _isBusy = false;
            SetButtonsEnabled(true);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SetButtonsEnabled(bool enabled)
    {
        LoginButton.IsEnabled = enabled;
        CancelLoginButton.IsEnabled = enabled;
        RegisterButton.IsEnabled = enabled;
        CancelRegisterButton.IsEnabled = enabled;
    }
}
