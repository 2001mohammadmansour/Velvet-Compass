import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './room.css';
import './reservation.css';
import { getCurrentUser } from './services/auth';
import { getMyProfile, updateMyProfile, changeMyPassword, updateStoredUsername } from './services/profile';

// CHANGED BY AI (2026-07-13): new self-service Edit Profile page — lets any logged-in user
// (guest or owner) change their username and phone number, and change their password. Email is
// intentionally read-only; the backend never accepts a change to it here either.
export default function EditProfile() {
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
      .catch((err) => { if (mounted) setLoadError(err.message || 'Unable to load your profile.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [user?.id]);

  const handleProfileSubmit = async (e) => {
    e.preventDefault();
    setProfileError('');
    setProfileSuccess('');
    if (!form.username.trim()) {
      setProfileError('Username can\'t be empty.');
      return;
    }
    setSavingProfile(true);
    try {
      const updated = await updateMyProfile({ username: form.username.trim(), phoneNumber: form.phoneNumber.trim() });
      setProfile(updated);
      setForm({ username: updated.username, phoneNumber: updated.phoneNumber });
      updateStoredUsername(updated.username);
      setProfileSuccess('Profile updated.');
    } catch (err) {
      setProfileError(err.message || 'Unable to update profile.');
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
      setPasswordError('Fill in both your current and new password.');
      return;
    }
    if (newPassword.length < 8 || !/\d/.test(newPassword)) {
      setPasswordError('New password must be at least 8 characters and include a digit.');
      return;
    }
    if (newPassword !== confirmPassword) {
      setPasswordError('New passwords don\'t match.');
      return;
    }
    setSavingPassword(true);
    try {
      await changeMyPassword({ currentPassword, newPassword });
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setPasswordSuccess('Password changed.');
    } catch (err) {
      setPasswordError(err.message || 'Unable to change password.');
    } finally {
      setSavingPassword(false);
    }
  };

  return (
    <div className="rooms-page">
      <div className="back-wrapper">
        <button type="button" className="back-btn" onClick={() => navigate(-1)}>← Back</button>
      </div>

      <h1 className="section-title">Edit Profile</h1>

      {!user?.id && <div className="empty-state"><p>Please log in to edit your profile.</p></div>}
      {loading && <p style={{ textAlign: 'center', marginBottom: 20 }}>Loading your profile...</p>}
      {loadError && <p style={{ textAlign: 'center', color: '#9b1c1c', marginBottom: 20 }}>{loadError}</p>}

      {user?.id && !loading && !loadError && profile && (
        <div style={{ maxWidth: 520, margin: '0 auto', display: 'flex', flexDirection: 'column', gap: 24 }}>
          <section className="section-card">
            <h2 className="section-title" style={{ fontSize: '1.1rem', marginBottom: 12 }}>Account Information</h2>
            <form onSubmit={handleProfileSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                Username
                <input
                  value={form.username}
                  onChange={(e) => setForm((f) => ({ ...f, username: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                Phone Number
                <input
                  value={form.phoneNumber}
                  onChange={(e) => setForm((f) => ({ ...f, phoneNumber: e.target.value }))}
                  placeholder="+1 555 123 4567"
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                Email <span style={{ color: '#9ca3af', fontStyle: 'italic' }}>(cannot be changed)</span>
                <input
                  value={profile.email}
                  readOnly
                  style={{ padding: '10px 12px', border: '1px solid #e2e8f0', borderRadius: 8, background: '#f8fafc', color: '#64748b', cursor: 'not-allowed' }}
                />
              </label>

              {profileError && <p style={{ color: '#9b1c1c', fontSize: 13, margin: 0 }}>{profileError}</p>}
              {profileSuccess && <p style={{ color: '#166534', fontSize: 13, margin: 0 }}>{profileSuccess}</p>}

              <button type="submit" className="cta" disabled={savingProfile} style={{ alignSelf: 'flex-start' }}>
                {savingProfile ? 'Saving...' : 'Save Changes'}
              </button>
            </form>
          </section>

          <section className="section-card">
            <h2 className="section-title" style={{ fontSize: '1.1rem', marginBottom: 12 }}>Change Password</h2>
            <form onSubmit={handlePasswordSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                Current Password
                <input
                  type="password"
                  value={passwordForm.currentPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, currentPassword: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                New Password <span style={{ color: '#9ca3af' }}>(min 8 characters, include a digit)</span>
                <input
                  type="password"
                  value={passwordForm.newPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, newPassword: e.target.value }))}
                  style={{ padding: '10px 12px', border: '1px solid #cbd5e1', borderRadius: 8 }}
                  required
                />
              </label>

              <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13 }}>
                Confirm New Password
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
                {savingPassword ? 'Saving...' : 'Change Password'}
              </button>
            </form>
          </section>
        </div>
      )}
    </div>
  );
}
