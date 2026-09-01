namespace DigitalSignage.Api.DTOs.Auth;

public enum ChangePasswordResult
{
    Success,
    UserNotFound,
    InvalidCurrentPassword,
    PasswordsDoNotMatch,
    SamePassword
}