import React, { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { getAdminUsers, getAdminUserBookings, suspendUser, unsuspendUser, getReviewDetail, deleteReview } from './services/hotels';

function formatMoney(value) {
  return `$${Math.round(Number(value) || 0).toLocaleString()}`;
}

const CATEGORY_KEYS = ['staff', 'location', 'facilities', 'cleanliness', 'comfort', 'value'];

// CHANGED BY AI (2026-07-13): please review — full review detail modal for admin moderation.
function ReviewDetailModal({ reviewId, onClose, onDeleted }) {
  const { t } = useTranslation();
  const [review, setReview] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    setError('');
    getReviewDetail(reviewId)
      .then((data) => { if (mounted) setReview(data); })
      .catch((err) => { if (mounted) setError(err.message || t('adminUsers.review.loadError')); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [reviewId, t]);

  async function handleDelete() {
    if (!window.confirm(t('adminUsers.review.deleteConfirm'))) return;
    setDeleting(true);
    try {
      await deleteReview(reviewId);
      onDeleted();
    } catch (err) {
      alert(t('adminUsers.review.deleteError') + (err.message || err));
      setDeleting(false);
    }
  }

  return (
    <div className="rv-overlay" onClick={onClose}>
      <div className="rv-modal" onClick={(e) => e.stopPropagation()}>
        <div className="rv-modal-header">
          <div>
            <h3>{t('adminUsers.review.title')}</h3>
            {review && <p>{review.roomName} · {review.hotelName}</p>}
          </div>
          <button className="rv-close" onClick={onClose} aria-label={t('common.close')}>×</button>
        </div>

        {loading && <p className="muted small">{t('adminUsers.review.loading')}</p>}
        {error && <p className="muted small" style={{ color: '#e05555' }}>{error}</p>}

        {review && (
          <>
            <div className="rv-score-preview">
              {t('adminUsers.review.overallScore')}
              <strong>{review.overallScore} / 10</strong>
            </div>

            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, margin: '14px 0' }}>
              {CATEGORY_KEYS.map((key) => (
                <div key={key} style={{ background: '#f3f4f6', borderRadius: 8, padding: '6px 12px', textAlign: 'center', minWidth: 90 }}>
                  <div style={{ fontSize: 11, color: '#6b7280', marginBottom: 2 }}>{t(`myBookings.review.categories.${key}.label`)}</div>
                  <div style={{ fontSize: 15, fontWeight: 700, color: '#1a2340' }}>{review[key]}</div>
                </div>
              ))}
            </div>

            <div className="rv-comment">
              <label>{t('adminUsers.review.guest')}</label>
              <p style={{ margin: '4px 0 12px' }}>{review.guestName} · {new Date(review.createdAt).toLocaleDateString()}</p>
              <label>{t('adminUsers.review.comment')}</label>
              <p style={{ margin: '4px 0' }}>{review.comment}</p>
            </div>

            <div className="rv-actions">
              <button type="button" className="back-btn" onClick={onClose} disabled={deleting}>{t('adminUsers.review.close')}</button>
              <button type="button" className="cta" style={{ background: '#c0392b' }} onClick={handleDelete} disabled={deleting}>
                {deleting ? t('adminUsers.review.deleting') : t('adminUsers.review.delete')}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

export default function AdminUsers() {
  const { t } = useTranslation();

  function formatSuspendedUntil(value) {
    if (!value) return t('adminUsers.indefinitely');
    const date = new Date(value);
    if (date.getFullYear() > 9000) return t('adminUsers.indefinitely'); // DateTimeOffset.MaxValue sentinel
    return t('adminUsers.until', { date: date.toLocaleDateString() });
  }

  const ROLE_FILTERS = [
    { key: 'all', label: t('adminUsers.roleFilters.all') },
    { key: 'guest', label: t('adminUsers.roleFilters.guest') },
    { key: 'owner', label: t('adminUsers.roleFilters.owner') },
    { key: 'admin', label: t('adminUsers.roleFilters.admin') },
  ];

  const SUSPEND_DURATIONS = [
    { key: '1', label: t('adminUsers.suspendDurations.1') },
    { key: '7', label: t('adminUsers.suspendDurations.7') },
    { key: '30', label: t('adminUsers.suspendDurations.30') },
    { key: 'indefinite', label: t('adminUsers.suspendDurations.indefinite') },
  ];
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const [search, setSearch] = useState('');
  const [suspendFormId, setSuspendFormId] = useState(null);
  const [suspendDuration, setSuspendDuration] = useState('7');
  const [actionSavingId, setActionSavingId] = useState(null);
  const [expandedUserId, setExpandedUserId] = useState(null);
  const [expandedBookings, setExpandedBookings] = useState([]);
  const [bookingsLoading, setBookingsLoading] = useState(false);
  const [bookingsError, setBookingsError] = useState('');
  const [openReviewId, setOpenReviewId] = useState(null);

  function load() {
    setLoading(true);
    setError('');
    return getAdminUsers()
      .then(setUsers)
      .catch((err) => setError(err.message || t('adminUsers.loadError')))
      .finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const counts = useMemo(() => {
    const c = { guest: 0, owner: 0, admin: 0 };
    users.forEach((u) => {
      const key = String(u.role || '').toLowerCase();
      if (c[key] !== undefined) c[key] += 1;
    });
    return c;
  }, [users]);

  const filtered = useMemo(() => {
    return users.filter((u) => {
      const roleOk = roleFilter === 'all' || String(u.role || '').toLowerCase() === roleFilter;
      const term = search.trim().toLowerCase();
      const searchOk = !term
        || String(u.username || '').toLowerCase().includes(term)
        || String(u.email || '').toLowerCase().includes(term);
      return roleOk && searchOk;
    });
  }, [users, roleFilter, search]);

  async function handleConfirmSuspend(userId) {
    setActionSavingId(userId);
    try {
      let until = null;
      if (suspendDuration !== 'indefinite') {
        const d = new Date();
        d.setDate(d.getDate() + Number(suspendDuration));
        until = d.toISOString();
      }
      await suspendUser(userId, until);
      setSuspendFormId(null);
      await load();
    } catch (err) {
      alert(t('adminUsers.suspendError') + (err.message || err));
    } finally {
      setActionSavingId(null);
    }
  }

  async function handleUnsuspend(userId) {
    setActionSavingId(userId);
    try {
      await unsuspendUser(userId);
      await load();
    } catch (err) {
      alert(t('adminUsers.unsuspendError') + (err.message || err));
    } finally {
      setActionSavingId(null);
    }
  }

  async function loadBookingsFor(userId) {
    setExpandedBookings([]);
    setBookingsError('');
    setBookingsLoading(true);
    try {
      const data = await getAdminUserBookings(userId);
      setExpandedBookings(data);
    } catch (err) {
      setBookingsError(err.message || t('adminUsers.loadBookingsError'));
    } finally {
      setBookingsLoading(false);
    }
  }

  function toggleExpand(userId) {
    if (expandedUserId === userId) {
      setExpandedUserId(null);
      return;
    }
    setExpandedUserId(userId);
    loadBookingsFor(userId);
  }

  // CHANGED BY AI (2026-07-13): please review — refreshes the currently-expanded user's bookings
  // after a review is deleted from the modal, so the row updates to "No review yet" right away.
  function handleReviewDeleted() {
    setOpenReviewId(null);
    if (expandedUserId != null) loadBookingsFor(expandedUserId);
  }

  if (loading) return <p className="admin-stat-sub">{t('adminUsers.loading')}</p>;
  if (error) return <p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p>;

  return (
    <div className="ha-root">
      <div className="admin-stats-row">
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('adminUsers.totalUsers')}</div>
          <div className="admin-stat-value">{users.length}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('adminUsers.guests')}</div>
          <div className="admin-stat-value">{counts.guest}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('adminUsers.owners')}</div>
          <div className="admin-stat-value">{counts.owner}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('adminUsers.admins')}</div>
          <div className="admin-stat-value">{counts.admin}</div>
        </div>
      </div>

      <div className="admin-card">
        <div className="admin-card-title">{t('adminUsers.allUsers')}</div>
        <div className="ha-controls">
          <div className="ha-sort-group">
            {ROLE_FILTERS.map((f) => (
              <button
                key={f.key}
                className={`ha-sort-btn ${roleFilter === f.key ? 'active' : ''}`}
                onClick={() => setRoleFilter(f.key)}
              >
                {f.label}
              </button>
            ))}
          </div>
          <input
            className="sr-filter-input"
            style={{ maxWidth: 240 }}
            placeholder={t('adminUsers.searchPlaceholder')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        <div className="ha-table-wrap">
          <table className="ha-table">
            <thead>
              <tr>
                <th>{t('adminUsers.username')}</th>
                <th>{t('adminUsers.email')}</th>
                <th>{t('adminUsers.phone')}</th>
                <th>{t('adminUsers.role')}</th>
                <th>{t('adminUsers.status')}</th>
                <th>{t('adminUsers.joined')}</th>
                <th className="ha-num">{t('adminUsers.bookings')}</th>
                <th className="ha-num">{t('adminUsers.paidToPlatform')}</th>
                <th>{t('adminUsers.hotelsOwned')}</th>
                <th>{t('adminUsers.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((u) => {
                const isAdmin = String(u.role || '').toLowerCase() === 'admin';
                const saving = actionSavingId === u.id;
                return (
                  <React.Fragment key={u.id}>
                  <tr
                    className={expandedUserId === u.id ? 'ha-row-active' : ''}
                    onClick={() => toggleExpand(u.id)}
                    style={{ cursor: 'pointer' }}
                  >
                    <td><strong>{u.username}</strong></td>
                    <td>{u.email}</td>
                    <td>{u.phoneNumber || '—'}</td>
                    <td>{t(`adminUsers.roles.${String(u.role || '').toLowerCase()}`, u.role)}</td>
                    <td>
                      <span className={`booking-status booking-status-${u.isSuspended ? 'cancelled' : 'confirmed'}`}>
                        {u.isSuspended ? t('adminUsers.suspendedUntil', { until: formatSuspendedUntil(u.suspendedUntil) }) : t('adminUsers.active')}
                      </span>
                    </td>
                    <td>{u.createdAt ? new Date(u.createdAt).toLocaleDateString() : '—'}</td>
                    <td className="ha-num">{u.bookingsCount}</td>
                    <td className="ha-num">{formatMoney(u.amountPaidToPlatform)}</td>
                    <td>{u.ownedHotelNames.length ? u.ownedHotelNames.join(', ') : '—'}</td>
                    <td onClick={(e) => e.stopPropagation()}>
                      {isAdmin ? (
                        <span className="muted small">—</span>
                      ) : (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
                          {u.isSuspended ? (
                            <button className="ha-sort-btn" disabled={saving} onClick={() => handleUnsuspend(u.id)}>
                              {saving ? '...' : t('adminUsers.unsuspend')}
                            </button>
                          ) : suspendFormId === u.id ? (
                            <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
                              <select value={suspendDuration} onChange={(e) => setSuspendDuration(e.target.value)}>
                                {SUSPEND_DURATIONS.map((d) => (
                                  <option key={d.key} value={d.key}>{d.label}</option>
                                ))}
                              </select>
                              <button className="ha-sort-btn active" disabled={saving} onClick={() => handleConfirmSuspend(u.id)}>
                                {saving ? '...' : t('adminUsers.confirm')}
                              </button>
                              <button className="ha-sort-btn" onClick={() => setSuspendFormId(null)}>{t('adminUsers.cancel')}</button>
                            </div>
                          ) : (
                            <button className="ha-sort-btn" onClick={() => setSuspendFormId(u.id)}>{t('adminUsers.suspend')}</button>
                          )}
                        </div>
                      )}
                    </td>
                  </tr>
                  {expandedUserId === u.id && (
                    <tr>
                      <td colSpan={10} style={{ background: '#f8fafc', padding: 16 }}>
                        {bookingsLoading && <p className="muted small">{t('adminUsers.loadingBookings')}</p>}
                        {bookingsError && <p className="muted small" style={{ color: '#e05555' }}>{bookingsError}</p>}
                        {!bookingsLoading && !bookingsError && (
                          expandedBookings.length === 0 ? (
                            <p className="muted small">{t('adminUsers.noBookingsForUser')}</p>
                          ) : (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                              {expandedBookings.map((b) => (
                                <div key={b.id} style={{ border: '1px solid #e2e8f0', borderRadius: 8, padding: '10px 14px', background: '#fff' }}>
                                  <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 8 }}>
                                    <strong>{b.hotelName}</strong>
                                    <span className={`booking-status booking-status-${b.status}`}>{t(`myBookings.statuses.${b.status}`, b.status)}</span>
                                  </div>
                                  <div className="muted small" style={{ marginTop: 4 }}>
                                    {b.checkIn} → {b.checkOut} · {formatMoney(b.totalAmount)}
                                  </div>
                                  {/* CHANGED BY AI (2026-07-13): please review — now shows the
                                      real review for this booking, if one exists; click it to see
                                      the full review and moderate (delete) it. */}
                                  {b.hasReview ? (
                                    <div
                                      className="muted small"
                                      style={{ marginTop: 6, cursor: 'pointer' }}
                                      onClick={() => setOpenReviewId(b.reviewId)}
                                      title={t('adminUsers.clickToViewReview')}
                                    >
                                      <strong style={{ color: '#2a3d66' }}>★ {b.reviewScore}/10</strong>
                                      {b.reviewComment && <span> — {b.reviewComment}</span>}
                                    </div>
                                  ) : (
                                    <div className="muted small" style={{ marginTop: 6, fontStyle: 'italic' }}>
                                      {t('adminUsers.noReviewYet')}
                                    </div>
                                  )}
                                </div>
                              ))}
                            </div>
                          )
                        )}
                      </td>
                    </tr>
                  )}
                  </React.Fragment>
                );
              })}
              {filtered.length === 0 && (
                <tr><td colSpan={10} className="ha-hint">{t('adminUsers.noUsersMatch')}</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {openReviewId != null && (
        <ReviewDetailModal
          reviewId={openReviewId}
          onClose={() => setOpenReviewId(null)}
          onDeleted={handleReviewDeleted}
        />
      )}
    </div>
  );
}
