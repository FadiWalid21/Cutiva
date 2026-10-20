using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CuttingOptimizer.Infrastructure.Email;

public static class AuthEmailTemplates
{
    public static string WelcomeSubject(bool arabic) =>
        arabic ? "مرحباً بك في CUTIVA" : "Welcome to CUTIVA";

    public static string ResetSubject(bool arabic) =>
        arabic ? "رمز إعادة تعيين كلمة المرور" : "Your password reset code";

    public static string PasswordChangedSubject(bool arabic) =>
        arabic ? "تم تغيير كلمة المرور" : "Your password was changed";

    public static string WelcomeHtml(
        string displayName,
        string email,
        string temporaryPassword,
        bool arabic)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? email : displayName;
        var greeting = arabic
            ? $"مرحباً {WebUtility.HtmlEncode(name)}،"
            : $"Hello {WebUtility.HtmlEncode(name)},";
        var body = arabic
            ? "تم إنشاء حساب ورشتك في CUTIVA. استخدم البيانات التالية لتسجيل الدخول، ثم غيّر كلمة المرور."
            : "Your CUTIVA workshop account is ready. Sign in with the credentials below, then change your password.";
        var emailLabel = arabic ? "البريد" : "Email";
        var passLabel = arabic ? "كلمة المرور المؤقتة" : "Temporary password";

        return Wrap(arabic, greeting, $@"
<p style=""margin:0 0 16px;color:#334155;line-height:1.6"">{body}</p>
<table role=""presentation"" style=""width:100%;border-collapse:collapse;margin:20px 0"">
  <tr><td style=""padding:10px 12px;background:#f8fafc;border:1px solid #e2e8f0;color:#64748b;font-size:12px"">{emailLabel}</td></tr>
  <tr><td style=""padding:12px;border:1px solid #e2e8f0;border-top:0;font-family:ui-monospace,monospace;font-size:15px;color:#0f172a"">{WebUtility.HtmlEncode(email)}</td></tr>
  <tr><td style=""padding:10px 12px;background:#f8fafc;border:1px solid #e2e8f0;border-top:0;color:#64748b;font-size:12px"">{passLabel}</td></tr>
  <tr><td style=""padding:12px;border:1px solid #e2e8f0;border-top:0;font-family:ui-monospace,monospace;font-size:18px;letter-spacing:0.08em;color:#0f172a;font-weight:700"">{WebUtility.HtmlEncode(temporaryPassword)}</td></tr>
</table>");
    }

    public static string ResetOtpHtml(string code, bool arabic)
    {
        var greeting = arabic ? "مرحباً،" : "Hello,";
        var body = arabic
            ? "استخدم الرمز التالي لإعادة تعيين كلمة المرور. صالح لمدة 10 دقائق."
            : "Use this code to reset your password. It expires in 10 minutes.";

        return Wrap(arabic, greeting, $@"
<p style=""margin:0 0 16px;color:#334155;line-height:1.6"">{body}</p>
<div style=""text-align:center;margin:28px 0"">
  <span style=""display:inline-block;padding:14px 28px;background:#0f172a;color:#fff;font-size:28px;letter-spacing:0.35em;font-weight:700;border-radius:10px;font-family:ui-monospace,monospace"">{WebUtility.HtmlEncode(code)}</span>
</div>");
    }

    public static string PasswordChangedHtml(string? displayName, bool arabic)
    {
        var name = string.IsNullOrWhiteSpace(displayName)
            ? (arabic ? "عزيزي المستخدم" : "there")
            : displayName.Trim();
        var greeting = arabic
            ? $"مرحباً {WebUtility.HtmlEncode(name)}،"
            : $"Hello {WebUtility.HtmlEncode(name)},";
        var body = arabic
            ? "تم تغيير كلمة مرور حسابك في CUTIVA بنجاح. إذا لم تقم بهذا التغيير، أعد تعيين كلمة المرور فوراً أو تواصل مع الدعم."
            : "Your CUTIVA account password was changed successfully. If you did not make this change, reset your password immediately or contact support.";
        var note = arabic
            ? "لأمانك، لا نرسل كلمة المرور الجديدة في البريد."
            : "For your security, we never include your password in this email.";

        return Wrap(arabic, greeting, $@"
<p style=""margin:0 0 16px;color:#334155;line-height:1.6"">{body}</p>
<p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5"">{note}</p>");
    }

    public static string HashOtp(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code.Trim()));
        return Convert.ToHexString(bytes);
    }

    public static string GenerateOtp()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6", CultureInfo.InvariantCulture);
    }

    public static string GenerateTemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        Span<char> chars = stackalloc char[12];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        chars[0] = 'A';
        chars[1] = 'a';
        chars[2] = '2';
        return new string(chars);
    }

    private static string Wrap(bool arabic, string greeting, string inner)
    {
        var dir = arabic ? "rtl" : "ltr";
        var footer = arabic
            ? "إذا لم تطلب هذا البريد فتجاهله."
            : "If you did not request this email, you can ignore it.";

        return $@"<!DOCTYPE html>
<html lang=""{(arabic ? "ar" : "en")}"" dir=""{dir}"">
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Tahoma,Arial,sans-serif"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f1f5f9;padding:32px 12px"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""100%"" style=""max-width:520px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e2e8f0"">
        <tr><td style=""padding:22px 28px;background:linear-gradient(135deg,#0f172a,#1e293b);color:#fff"">
          <div style=""font-size:11px;letter-spacing:0.2em;opacity:0.7;text-transform:uppercase"">CUTIVA</div>
          <div style=""font-size:22px;font-weight:700;margin-top:4px"">{WebUtility.HtmlEncode(greeting)}</div>
        </td></tr>
        <tr><td style=""padding:28px"">{inner}
          <p style=""margin:24px 0 0;color:#94a3b8;font-size:12px;line-height:1.5"">{footer}</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
    }
}
