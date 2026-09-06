import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import './ownerDashboard.css';
import { submitHotelRequest, getMyHotelRequests, REQUEST_FIELDS } from './services/hotelRequests';
import { fileToResizedDataUrl } from './data/imageUtil';
import { getCurrentUser } from './services/auth';

const MAX_DOC_BYTES = 8 * 1024 * 1024; // 8MB source cap (image is downscaled before storing)

const emptyForm = { hotelName: '', address: '', stars: 0 };

function StarPicker({ value, onChange }) {
  return (
    <div style={{ display: 'flex', gap: 6 }}>
      {[1, 2, 3, 4, 5].map((n) => (
        <span
          key={n}
          onClick={() => onChange(n)}
          style={{ fontSize: 28, cursor: 'pointer', color: n <= value ? '#f59e0b' : '#d1d5db', userSelect: 'none', lineHeight: 1 }}
        >
          ★
        </span>
      ))}
    </div>
  );
}

function StatusBadge({ status }) {
  const { t } = useTranslation();
  const map = {
    pending: { label: t('ownerRequests.statuses.pending'), cls: 'orq-badge-pending' },
    approved: { label: t('ownerRequests.statuses.approved'), cls: 'orq-badge-approved' },
    rejected: { label: t('ownerRequests.statuses.rejected'), cls: 'orq-badge-rejected' },
  };
  const s = map[status] || map.pending;
  return <span className={`orq-badge ${s.cls}`}>{s.label}</span>;
}

export default function OwnerRequests({ embedded = false }) {
  const { t } = useTranslation();
  const owner = useMemo(() => {
    const user = getCurrentUser() || {};
    return {
      ownerName: user.username || 'Owner',
      ownerEmail: user.email || '',
      hotelId: user.hotelId || null,
      hotelName: user.hotelName || '',
      city: user.city || '',
      address: user.address || '',
      phoneNumber: user.phoneNumber || '',
      description: user.description || '',
      stars: Number(user.stars) || 0,
    };
  }, []);

  // Owners can only request edits to their existing hotel; new-hotel registration was removed.
  const [form, setForm] = useState(emptyForm);
  const [doc, setDoc] = useState(null); // { name, dataUrl }
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [myRequests, setMyRequests] = useState([]);
  const [loadError, setLoadError] = useState('');

  const loadRequests = useCallback(() => {
    return getMyHotelRequests()
      .then(setMyRequests)
      .catch((err) => setLoadError(err.message || t('ownerRequests.errors.loadRequestsFailed')));
  }, [t]);

  useEffect(() => { loadRequests(); }, [loadRequests]);

  // Prefill with the owner's current hotel info
  useEffect(() => {
    setForm({
      hotelName: owner.hotelName || '',
      address: owner.address || '',
      stars: owner.stars || 0,
    });
    setError('');
    setSuccess('');
  }, [owner]);

  function updateField(key, value) {
    setForm((prev) => ({ ...prev, [key]: value }));
    setSuccess('');
  }

  async function handleDoc(e) {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > MAX_DOC_BYTES) {
      setError(t('ownerRequests.errors.documentTooLarge'));
      e.target.value = '';
      return;
    }
    try {
      const dataUrl = await fileToResizedDataUrl(file);
      setDoc({ name: file.name, dataUrl });
      setError('');
    } catch {
      setError(t('ownerRequests.errors.documentUnreadable'));
    }
    e.target.value = '';
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    if (!doc) {
      setError(t('ownerRequests.errors.documentRequired'));
      return;
    }
    if (!form.hotelName.trim()) {
      setError(t('ownerRequests.errors.hotelNameRequired'));
      return;
    }
    if (!owner.hotelId) {
      setError(t('ownerRequests.errors.noApprovedHotel'));
      return;
    }
    setSubmitting(true);
    try {
      await submitHotelRequest({
        type: 'edit',
        hotelId: owner.hotelId,
        hotelName: form.hotelName.trim(),
        address: form.address.trim(),
        stars: form.stars || null,
        document: doc,
      });
    } catch (err) {
      setError(err.message || t('ownerRequests.errors.submitFailed'));
      setSubmitting(false);
      return;
    }
    setDoc(null);
    setSuccess(t('ownerRequests.success.edited'));
    setSubmitting(false);
    await loadRequests();
  }

  return (
    <div className={embedded ? "" : "owner-dashboard"}>
      {!embedded && (
        <>
          <header className="od-header">
            <h1>{t('ownerRequests.title')}</h1>
            <p className="muted">{t('ownerRequests.subtitle')}</p>
          </header>

          <div style={{ marginBottom: 14 }}>
            <Link to="/ownerhome" className="cta" style={{ textDecoration: 'none', display: 'inline-block' }}>
              {t('ownerRequests.backToOwnerHome')}
            </Link>
          </div>
        </>
      )}

      {error && <div className="orq-alert orq-alert-error">{error}</div>}
      {success && <div className="orq-alert orq-alert-success">{success}</div>}

      <section className="od-row">
        <form onSubmit={handleSubmit}>
          <div style={{ display: 'grid', gap: 12, gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))' }}>
            <label>
              <div className="small muted" style={{ marginBottom: 4 }}>{t('ownerRequests.hotelName')}</div>
              <input value={form.hotelName}
                onChange={(e) => updateField('hotelName', e.target.value)}
                placeholder={t('ownerRequests.hotelNamePlaceholder')} required className="orq-input" />
            </label>
            <label style={{ gridColumn: '1 / -1' }}>
              <div className="small muted" style={{ marginBottom: 4 }}>{t('ownerRequests.address')}</div>
              <input value={form.address} onChange={(e) => updateField('address', e.target.value)}
                placeholder={t('ownerRequests.addressPlaceholder')} className="orq-input" />
            </label>
            <div style={{ gridColumn: '1 / -1' }}>
              <div className="small muted" style={{ marginBottom: 6 }}>{t('ownerRequests.hotelStars')}</div>
              <StarPicker value={form.stars} onChange={(n) => updateField('stars', n)} />
            </div>

            <label style={{ gridColumn: '1 / -1' }}>
              <div className="small muted" style={{ marginBottom: 4 }}>{t('ownerRequests.documentImage')}</div>
              <input type="file" accept="image/*" onChange={handleDoc} />
              {doc && (
                <div className="orq-doc-preview">
                  <img src={doc.dataUrl} alt="Document" />
                  <span className="small muted">{doc.name}</span>
                </div>
              )}
            </label>
          </div>

          <div style={{ marginTop: 14 }}>
            <button className="save-btn" type="submit" disabled={submitting}>
              {submitting ? t('ownerRequests.submitting') : t('ownerRequests.submitRequest')}
            </button>
          </div>
        </form>
      </section>

      <section className="od-row" style={{ marginTop: 18 }}>
        <h2 style={{ marginTop: 0 }}>{t('ownerRequests.myRequests')}</h2>
        {loadError && <p className="muted small" style={{ color: '#e05555' }}>{loadError}</p>}
        {myRequests.length === 0 && !loadError && <p className="muted small">{t('ownerRequests.noRequestsYet')}</p>}
        <div className="orq-list">
          {myRequests.map((r) => (
            <div key={r.id} className="orq-card">
              <div className="orq-card-head">
                <div>
                  <strong>{r.type === 'create' ? t('ownerRequests.newHotel') : t('ownerRequests.editHotel')}</strong>
                  {' · '}
                  <span className="muted small">{new Date(r.createdAt).toLocaleString()}</span>
                </div>
                <StatusBadge status={r.status} />
              </div>
              <div className="orq-changes">
                {REQUEST_FIELDS.map((f) =>
                  r.changes?.[f.key] ? (
                    <div key={f.key} className="orq-change-line">
                      <span className="muted small">{t(`ownerRequests.fields.${f.key}`, f.label)}:</span>{' '}
                      {f.render ? f.render(r.changes[f.key]) : r.changes[f.key]}
                    </div>
                  ) : null
                )}
              </div>
              {r.status === 'rejected' && r.rejectionReason && (
                <div className="orq-reject-reason">
                  <strong>{t('ownerRequests.rejectionReason')}</strong> {r.rejectionReason}
                </div>
              )}
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
