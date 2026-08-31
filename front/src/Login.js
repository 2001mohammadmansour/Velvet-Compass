
import React, { useState } from "react";
import "./signUp.css";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import LanguageToggle from "./LanguageToggle";

export default function Login() {
  const { t } = useTranslation();
  const [form, setForm] = useState({ email: "", password: "", remember: false });
  const [loading, setLoading] = useState(false);
  // When the account has 2FA enabled, signInUser returns a challenge instead of a session and
  // we switch to the code-entry step below.
  const [twoFactor, setTwoFactor] = useState(null); // { challengeToken }
  const [code, setCode] = useState("");
  const navigate = useNavigate();
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  // Stores the session and, for owners, looks up which hotel they run (the login response
  // doesn't carry it). Shared by the password step and the 2FA step.
  const finishLogin = async (res) => {
    let next = null;
    try {
      const existingRaw = localStorage.getItem('mock_auth_user');
      const existing = existingRaw ? JSON.parse(existingRaw) : {};
      next = {
        ...res,
        user: {
          ...(existing?.user || {}),
          ...(res?.user || {}),
          hotelId: res?.user?.hotelId || existing?.user?.hotelId || null,
          hotelName: res?.user?.hotelName || existing?.user?.hotelName || null,
        },
      };
      localStorage.setItem('mock_auth_user', JSON.stringify(next));
    } catch (e) {}

    if (next?.user?.role === 'hotel_owner') {
      try {
        const { getMyHotels } = await import("./services/hotels");
        const myHotels = await getMyHotels();
        const ownedHotel = myHotels[0];
        if (ownedHotel) {
          next = { ...next, user: { ...next.user, hotelId: ownedHotel.hotelId, hotelName: ownedHotel.hotelName } };
          localStorage.setItem('mock_auth_user', JSON.stringify(next));
        }
      } catch (e) { /* owner may not have an approved hotel yet */ }
    }

    navigate('/');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { signInUser } = await import("./services/auth");
      const res = await signInUser({ email: form.email.trim(), password: form.password });
      if (res?.requiresTwoFactor) {
        setTwoFactor({ challengeToken: res.challengeToken });
        return;
      }
      await finishLogin(res);
    } catch (err) {
      alert('Login failed: ' + (err.message || err));
    } finally {
      setLoading(false);
    }
  };

  const handleVerify = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { verifyTwoFactor } = await import("./services/auth");
      const trimmed = code.trim();
      // A 6-digit string is a TOTP code; anything else is treated as a recovery code.
      const isTotp = /^\d{6}$/.test(trimmed);
      const res = await verifyTwoFactor({
        challengeToken: twoFactor.challengeToken,
        code: isTotp ? trimmed : null,
        recoveryCode: isTotp ? null : trimmed,
      });
      await finishLogin(res);
    } catch (err) {
      alert('Login failed: ' + (err.message || err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <div className="overlay" />
      <Link to="/" className="auth-back-btn">{t('common.backButton')}</Link>
      <LanguageToggle className="auth-lang-toggle" />
      <main className="card">
        {!twoFactor ? (
          <>
            <h1>{t('auth.login.welcomeBack')}</h1>
            <p className="subtitle">{t('auth.login.subtitle')}</p>
            <form onSubmit={handleSubmit} noValidate>
              <label>
                {t('auth.login.email')}
                <input
                  type="email"
                  name="email"
                  placeholder={t('auth.login.emailPlaceholder')}
                  value={form.email}
                  onChange={handleChange}
                  autoComplete="email"
                  required
                />
              </label>
              <label>
                {t('auth.login.password')}
                <input
                  type="password"
                  name="password"
                  placeholder={t('auth.login.passwordPlaceholder')}
                  value={form.password}
                  onChange={handleChange}
                  autoComplete="current-password"
                  required
                />
              </label>
              <label className="checkbox-row">
                <input
                  type="checkbox"
                  name="remember"
                  checked={form.remember}
                  onChange={handleChange}
                />
                <span>{t('auth.login.rememberMe')}</span>
              </label>
              <button type="submit" disabled={loading}>
                {loading ? t('auth.login.loggingIn') : t('auth.login.logIn')}
              </button>
              <button type="button" className="link-button" onClick={() => navigate("/signup")}>
                {t('auth.login.createAccount')}
              </button>
            </form>
          </>
        ) : (
          <>
            <h1>{t('auth.login.twoFactorTitle')}</h1>
            <p className="subtitle">{t('auth.login.twoFactorSubtitle')}</p>
            <form onSubmit={handleVerify} noValidate>
              <label>
                {t('auth.login.twoFactorCode')}
                <input
                  type="text"
                  name="code"
                  inputMode="text"
                  autoComplete="one-time-code"
                  placeholder={t('auth.login.twoFactorCodePlaceholder')}
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  required
                  autoFocus
                />
              </label>
              <button type="submit" disabled={loading}>
                {loading ? t('auth.login.verifying') : t('auth.login.verify')}
              </button>
              <button
                type="button"
                className="link-button"
                onClick={() => { setTwoFactor(null); setCode(""); }}
              >
                {t('common.backButton')}
              </button>
            </form>
          </>
        )}
      </main>
    </div>
  );
}
