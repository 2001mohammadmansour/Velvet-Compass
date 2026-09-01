import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './room.css';
import './reservation.css';
import { getCurrentUser } from './services/auth';
import {
  getMyProfile, updateMyProfile, changeMyPassword, updateStoredUsername,
  setupTwoFactor, enableTwoFactor, disableTwoFactor, regenerateRecoveryCodes,
} from './services/profile';

// CHANGED BY AI (2026-07-13): new self-service Edit Profile page — lets any logged-in user
// (guest or owner) change their username and phone number, and change their password. Email is
// intentionally read-only; the backend never accepts a change to it here either.
export default function EditProfile() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const user = getCurrentUser();

  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');

  const [form, setForm] = useState({ username: '', phoneNumber: '' });
  const [savingProfile, setSavingProfile] = useState(false);
  const [profileError, setProfileError] = useState('');
  const [profileSuccess, setProfileSuccess] = useState('');

  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [savingPassword, setSavingPassword] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  const [passwordSuccess, setPasswordSuccess] = useState('');

  useEffect(() => {
    if (!user?.id) { setLoading(false); return; }
    let mounted = true;
    getMyProfile()
      .then((p) => {
        if (!mounted) return;
        setProfile(p);
        setForm({ username: p.username, phoneNumber: p.phoneNumber });
      })
      .catch((err) => { if (mounted) setLoadError(err.message || t('editProfile.loadError')); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [user?.id, t]);

  const handleProfileSubmit = async (e) => {
    e.preventDefault();
    setProfileError('');
    setProfileSuccess('');
    if (!form.username.trim()) {
      setProfileError(t('editProfile.errors.usernameEmpty'));
      return;
    }
    setSavingProfile(true);
    try {
      const updated = await updateMyProfile({ username: form.username.trim(), phoneNumber: form.phoneNumber.trim() });
      setProfile(updated);
      setForm({ username: updated.username, phoneNumber: updated.phoneNumber });
      updateStoredUsername(updated.username);
      setProfileSuccess(t('editProfile.success.profileUpdated'));
    } catch (err) {
      setProfileError(err.message || t('editProfile.errors.profileUpdateFailed'));
    } finally {
      setSavingProfile(false);
    }
  };

  const handlePasswordSubmit = async (e) => {
    e.preventDefault();
    setPasswordError('');
    setPasswordSuccess('');
    const { currentPassword, newPassword, confirmPassword } = passwordForm;
    if (!currentPassword || !newPassword) {
      setPasswordError(t('editProfile.errors.bothPasswordsRequired'));
      return;
    }
    if (newPassword.length < 8 || !/\d/.test(newPassword)) {
      setPasswordError(t('editProfile.errors.passwordTooWeak'));
      return;
    }
    if (newPassword !== confirmPassword) {
      setPasswordError(t('editProfile.errors.passwordsDontMatch'));
      return;
    }
    setSavingPassword(true);
    try {
      await changeMyPassword({ currentPassword, newPassword });
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setPasswordSuccess(t('editProfile.success.passwordChanged'));
    } catch (err) {
      setPasswordError(err.message || t('editProfile.errors.passwordChangeFailed'));
    } finally {
      setSavingPassword(false);
    }
  };

  return (
    <div className="rooms-page">
      <div className="back-wrapper">
        <button type="button" className="back-btn" onClick={() => navigate(-1)}>{t('editProfile.back')}</button>
      </div>

      <h1 className="section-title">{t('editProfile.title')}</h1>

      {!user?.id && <div className="empty-state"><p>{t('editProfile.pleaseLogin')}</p></div>}
      {loading && <p style={{ textAlign: 'center', marginBottom: 20 }}>{t('editProfile.loading')}</p>}
      {loadError && <p style={{ textAlign: 'center', color: '#9b1c1c', marginBottom: 20 }}>{loadError}</p>}

      {user?.id && !loading && !loadError && profile && (
        <div style={{ maxWidth: 520, margin: '0 auto', display: 'flex', flexDirection: 'column', gap: 24 }}>
          <section className="section-card">
            <h2 className="section-title" style={{ fontSize: '1.1rem', marginBottom: 12 }}>{t('editProfile.accountInformation')}</h2>
            <form onSubmit={handleProfileSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                {t('editProfile.username')}
                <input
                  value={form.username}
                  onChange={(e) => setForm((f) => ({ ...f, username: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                {t('editProfile.phoneNumber')}
                <input
                  value={form.phoneNumber}
                  onChange={(e) => setForm((f) => ({ ...f, phoneNumber: e.target.value }))}
                  placeholder="+1 555 123 4567"
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                {t('editProfile.email')} <span style={{ color: '#9ca3af', fontStyle: 'italic' }}>{t('editProfile.cannotBeChanged')}</span>
                <input
                  value={profile.email}
                  readOnly
                  style={{ padding: '10px 12px', border: '1px solid #e2e8f0', borderRadius: 8, background: '#f8fafc', color: '#64748b', cursor: 'not-allowed' }}
                />
              </label>

              {profileError && <p style={{ color: '#9b1c1c', fontSize: 13, margin: 0 }}>{profileError}</p>}
              {profileSuccess && <p style={{ color: '#166534', fontSize: 13, margin: 0 }}>{profileSuccess}</p>}

              <button type="submit" className="cta" disabled={savingProfile} style={{ alignSelf: 'flex-start' }}>
                {savingProfile ? t('editProfile.saving') : t('editProfile.saveChanges')}
              </button>
            </form>
          </section>

          <section className="section-card">
            <h2 className="section-title" style={{ fontSize: '1.1rem', marginBottom: 12 }}>{t('editProfile.changePassword')}</h2>
            <form onSubmit={handlePasswordSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                {t('editProfile.currentPassword')}
                <input
                  type="password"
                  value={passwordForm.currentPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, currentPassword: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                {t('editProfile.newPassword')} <span style={{ color: '#9ca3af' }}>{t('editProfile.newPasswordHint')}</span>
                <input
                  type="password"
                  value={passwordForm.newPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, newPassword: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                {t('editProfile.confirmNewPassword')}
                <input
                  type="password"
                  value={passwordForm.confirmPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, confirmPassword: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              {passwordError && <p style={{ color: '#9b1c1c', fontSize: 13, margin: 0 }}>{passwordError}</p>}
              {passwordSuccess && <p style={{ color: '#166534', fontSize: 13, margin: 0 }}>{passwordSuccess}</p>}

              <button type="submit" className="cta" disabled={savingPassword} style={{ alignSelf: 'flex-start' }}>
                {savingPassword ? t('editProfile.saving') : t('editProfile.changePasswordBtn')}
              </button>
            </form>
          </section>

          <TwoFactorSection
            enabled={profile.twoFactorEnabled}
            onChange={(next) => setProfile((p) => ({ ...p, twoFactorEnabled: next }))}
          />
        </div>
      )}
    </div>
  );
}

const inputStyle = { padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 };
const errStyle = { color: '#9b1c1c', fontSize: 13, margin: 0 };
const okStyle = { color: '#166534', fontSize: 13, margin: 0 };

// Self-service 2FA (TOTP authenticator app). Enabling is a 3-step flow: fetch a secret/QR,
// scan it, then confirm a 6-digit code — the backend only flips the flag once the code checks
// out, and hands back one-time recovery codes we show exactly once here.
function TwoFactorSection({ enabled, onChange }) {
  const { t } = useTranslation();
  const [mode, setMode] = useState('idle'); // idle | setup | disable
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [setupData, setSetupData] = useState(null); // { manualEntryKey, qrCodeImageBase64 }
  const [code, setCode] = useState('');
  const [password, setPassword] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState(null);

  const reset = () => {
    setMode('idle'); setBusy(false); setError('');
    setSetupData(null); setCode(''); setPassword('');
  };

  const beginSetup = async () => {
    setBusy(true); setError('');
    try {
      setSetupData(await setupTwoFactor());
      setMode('setup');
    } catch (err) {
      setError(err.message || t('editProfile.twoFactor.errors.generic'));
    } finally {
      setBusy(false);
    }
  };

  const confirmEnable = async (e) => {
    e.preventDefault();
    setBusy(true); setError('');
    try {
      const res = await enableTwoFactor(code.trim());
      setRecoveryCodes(res?.recoveryCodes || res || []);
      onChange(true);
      reset();
    } catch (err) {
      setError(err.message || t('editProfile.twoFactor.errors.invalidCode'));
    } finally {
      setBusy(false);
    }
  };

  const confirmDisable = async (e) => {
    e.preventDefault();
    setBusy(true); setError('');
    try {
      await disableTwoFactor(password);
      onChange(false);
      setRecoveryCodes(null);
      reset();
    } catch (err) {
      setError(err.message || t('editProfile.twoFactor.errors.wrongPassword'));
    } finally {
      setBusy(false);
    }
  };

  const regenerate = async () => {
    setBusy(true); setError('');
    try {
      const res = await regenerateRecoveryCodes();
      setRecoveryCodes(res?.recoveryCodes || res || []);
    } catch (err) {
      setError(err.message || t('editProfile.twoFactor.errors.generic'));
    } finally {
      setBusy(false);
    }
  };

  return (
    <section className="section-card">
      <h2 className="section-title" style={{ fontSize: '1.1rem', marginBottom: 12 }}>
        {t('editProfile.twoFactor.title')}
      </h2>

      <p style={{ fontSize: 13, color: '#475569', marginTop: 0 }}>
        {enabled ? t('editProfile.twoFactor.statusOn') : t('editProfile.twoFactor.statusOff')}
      </p>

      {error && <p style={errStyle}>{error}</p>}

      {/* One-time display of recovery codes after enabling / regenerating. */}
      {recoveryCodes && (
        <div style={{ background: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: 8, padding: 12, marginBottom: 14 }}>
          <p style={{ ...okStyle, marginBottom: 8 }}>{t('editProfile.twoFactor.recoveryIntro')}</p>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 4, fontFamily: 'monospace', fontSize: 13 }}>
            {recoveryCodes.map((c) => <span key={c}>{c}</span>)}
          </div>
          <button type="button" className="link-button" onClick={() => setRecoveryCodes(null)} style={{ marginTop: 8 }}>
            {t('editProfile.twoFactor.dismissCodes')}
          </button>
        </div>
      )}

      {mode === 'idle' && !enabled && (
        <button type="button" className="cta" disabled={busy} onClick={beginSetup} style={{ alignSelf: 'flex-start' }}>
          {busy ? t('editProfile.saving') : t('editProfile.twoFactor.enableBtn')}
        </button>
      )}

      {mode === 'idle' && enabled && (
        <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
          <button type="button" className="cta" disabled={busy} onClick={regenerate}>
            {t('editProfile.twoFactor.regenerateBtn')}
          </button>
          <button type="button" className="link-button" disabled={busy} onClick={() => setMode('disable')}>
            {t('editProfile.twoFactor.disableBtn')}
          </button>
        </div>
      )}

      {mode === 'setup' && setupData && (
        <form onSubmit={confirmEnable} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <p style={{ fontSize: 13, margin: 0 }}>{t('editProfile.twoFactor.scanInstruction')}</p>
          <img
            src={setupData.qrCodeImageBase64}
            alt="2FA QR code"
            style={{ width: 180, height: 180, alignSelf: 'center', border: '1px solid #e2e8f0', borderRadius: 8 }}
          />
          <p style={{ fontSize: 12, color: '#64748b', margin: 0 }}>
            {t('editProfile.twoFactor.manualKey')} <code>{setupData.manualEntryKey}</code>
          </p>
          <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
            {t('editProfile.twoFactor.codeLabel')}
            <input
              value={code}
              onChange={(e) => setCode(e.target.value)}
              inputMode="numeric"
              autoComplete="one-time-code"
              placeholder="123456"
              style={inputStyle}
              required
              autoFocus
            />
          </label>
          <div style={{ display: 'flex', gap: 10 }}>
            <button type="submit" className="cta" disabled={busy}>
              {busy ? t('editProfile.saving') : t('editProfile.twoFactor.confirmBtn')}
            </button>
            <button type="button" className="link-button" disabled={busy} onClick={reset}>
              {t('editProfile.twoFactor.cancelBtn')}
            </button>
          </div>
        </form>
      )}

      {mode === 'disable' && (
        <form onSubmit={confirmDisable} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
            {t('editProfile.twoFactor.passwordLabel')}
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              style={inputStyle}
              required
              autoFocus
            />
          </label>
          <div style={{ display: 'flex', gap: 10 }}>
            <button type="submit" className="cta" disabled={busy}>
              {busy ? t('editProfile.saving') : t('editProfile.twoFactor.confirmDisableBtn')}
            </button>
            <button type="button" className="link-button" disabled={busy} onClick={reset}>
              {t('editProfile.twoFactor.cancelBtn')}
            </button>
          </div>
        </form>
      )}
    </section>
  );
}
