import { Fragment, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { getAdminDashboard, getRoomTypesForHotel, deleteReview } from './services/hotels';
import { getRoomReviews } from './services/guest';

function formatMoney(value) {
  return `$${Math.round(Number(value) || 0).toLocaleString()}`;
}

// CHANGED BY AI (2026-07-15): please review. New All Time / Year / Month filter for the whole
// analytics tab — scopes revenue, booking stats, and both ranking tables to the selected period
// (bookings filtered server-side by check-in date; see getAdminDashboard).
function PeriodFilter({ mode, setMode, year, setYear, month, setMonth }) {
  const { t } = useTranslation();
  const monthNames = t('common.fullMonths', { returnObjects: true });
  const now = new Date();
  const years = [now.getFullYear(), now.getFullYear() - 1, now.getFullYear() - 2];

  return (
    <div className="admin-card" style={{ display: 'flex', flexWrap: 'wrap', gap: 10, alignItems: 'center' }}>
      <div className="ha-sort-row" style={{ display: 'flex', gap: 8 }}>
        {['all', 'year', 'month'].map((m) => (
          <button
            key={m}
            className={`ha-sort-btn ${mode === m ? 'active' : ''}`}
            onClick={() => setMode(m)}
          >
            {m === 'all' ? t('hotelsAnalytics.allTime') : m === 'year' ? t('hotelsAnalytics.year') : t('hotelsAnalytics.month')}
          </button>
        ))}
      </div>

      {mode !== 'all' && (
        <select className="sr-filter-input" style={{ width: 110 }} value={year} onChange={(e) => setYear(Number(e.target.value))}>
          {years.map((y) => <option key={y} value={y}>{y}</option>)}
        </select>
      )}

      {mode === 'month' && (
        <select className="sr-filter-input" style={{ width: 150 }} value={month} onChange={(e) => setMonth(Number(e.target.value))}>
          {monthNames.map((label, i) => <option key={label} value={i + 1}>{label}</option>)}
        </select>
      )}
    </div>
  );
}

// CHANGED BY AI (2026-07-13): please review — one room type's card in the hotel drill-down, with
// its reviews shown/moderated inline (view + delete). Manages its own reviews fetch/expand state
// so opening one room's reviews doesn't affect its siblings.
function RoomTypeCard({ hotelId, room }) {
  const { t } = useTranslation();
  const [reviewsOpen, setReviewsOpen] = useState(false);
  const [reviewData, setReviewData] = useState(null);
  const [reviewsError, setReviewsError] = useState('');
  const [loadingReviews, setLoadingReviews] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  async function loadReviews() {
    setLoadingReviews(true);
    setReviewsError('');
    try {
      const data = await getRoomReviews(hotelId, room.id);
      setReviewData(data);
    } catch (err) {
      setReviewsError(err.message || t('hotelsAnalytics.loadReviewsError'));
    } finally {
      setLoadingReviews(false);
    }
  }

  function toggleReviews() {
    const next = !reviewsOpen;
    setReviewsOpen(next);
    if (next && !reviewData) loadReviews();
  }

  async function handleDelete(reviewId) {
    if (!window.confirm(t('hotelsAnalytics.deleteReviewConfirm'))) return;
    setDeletingId(reviewId);
    try {
      await deleteReview(reviewId);
      await loadReviews();
    } catch (err) {
      alert(t('hotelsAnalytics.deleteReviewError') + (err.message || err));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div className="ha-room-card">
      <div className="ha-room-card-header">
        <div className="ha-room-card-main">
          <strong>{room.name}</strong>
          <span className="muted small">{room.capacity} guest{room.capacity !== 1 ? 's' : ''} · {formatMoney(room.price)}{t('hotelsAnalytics.perNight')}</span>
        </div>
        <button className="ha-sort-btn" onClick={toggleReviews}>
          {room.reviewCount > 0 ? `★ ${room.avgScore}/10 · ${room.reviewCount} review${room.reviewCount !== 1 ? 's' : ''}` : t('hotelsAnalytics.noReviewsYet')}
          <span style={{ marginInlineStart: 6 }}>{reviewsOpen ? '▲' : '▼'}</span>
        </button>
      </div>

      {reviewsOpen && (
        <div className="ha-review-list">
          {loadingReviews && <p className="ha-hint">{t('hotelsAnalytics.loadingReviews')}</p>}
          {reviewsError && <p className="ha-hint" style={{ color: '#e05555' }}>{reviewsError}</p>}
          {!loadingReviews && !reviewsError && reviewData && reviewData.reviews.length === 0 && (
            <p className="ha-hint">{t('hotelsAnalytics.noReviewsYet')}</p>
          )}
          {!loadingReviews && !reviewsError && reviewData && reviewData.reviews.map((r) => (
            <div key={r.id} className="ha-review-item">
              <div className="ha-review-item-header">
                <span className="ha-review-item-name">{r.guestName}</span>
                <span className="ha-review-item-score">{r.overallScore}/10</span>
                <span className="ha-review-item-date">{new Date(r.createdAt).toLocaleDateString()}</span>
                <button
                  className="ha-review-delete-btn"
                  disabled={deletingId === r.id}
                  onClick={() => handleDelete(r.id)}
                  title={t('hotelsAnalytics.deleteReviewTitle')}
                >
                  {deletingId === r.id ? '...' : '🗑'}
                </button>
              </div>
              <p className="ha-review-item-comment">{r.comment}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// New room-type drill-down for a hotel row in the admin analytics ranking tables (previously
// there was no way to see a hotel's rooms here at all).
function HotelRoomsRow({ hotelId }) {
  const { t } = useTranslation();
  const [rooms, setRooms] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    let mounted = true;
    getRoomTypesForHotel(hotelId)
      .then((list) => { if (mounted) setRooms(list); })
      .catch((err) => { if (mounted) setError(err.message || t('hotelsAnalytics.loadRoomsError')); });
    return () => { mounted = false; };
  }, [hotelId, t]);

  return (
    <tr className="ha-row-active">
      <td colSpan={5} style={{ padding: '12px 14px' }}>
        {error && <p className="ha-hint" style={{ color: '#e05555' }}>{error}</p>}
        {!error && !rooms && <p className="ha-hint">{t('hotelsAnalytics.loadingRooms')}</p>}
        {!error && rooms && rooms.length === 0 && <p className="ha-hint">{t('hotelsAnalytics.noRoomTypesYet')}</p>}
        {!error && rooms && rooms.length > 0 && (
          <div className="ha-room-list">
            {rooms.map((r) => (
              <RoomTypeCard key={r.id} hotelId={hotelId} room={r} />
            ))}
          </div>
        )}
      </td>
    </tr>
  );
}

/* ── small CSS bar chart ── */
function BarChart({ items, valueKey, labelKey, color = '#6C8BC7', formatValue }) {
  const max = Math.max(1, ...items.map((it) => Number(it[valueKey]) || 0));
  return (
    <div className="ha-chart">
      {items.map((it, i) => {
        const v = Number(it[valueKey]) || 0;
        const pct = Math.round((v / max) * 100);
        return (
          <div className="ha-bar-row" key={it.id ?? it[labelKey] ?? i}>
            <div className="ha-bar-label" title={it[labelKey]}>{it[labelKey]}</div>
            <div className="ha-bar-track">
              <div className="ha-bar-fill" style={{ width: `${pct}%`, background: color }}>
                <span className="ha-bar-value">{formatValue ? formatValue(v) : v}</span>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}

function RankingTable({ title, icon, items, valueKey, valueLabel, formatValue }) {
  const { t } = useTranslation();
  const [expandedHotelId, setExpandedHotelId] = useState(null);

  return (
    <div className="admin-card">
      <div className="admin-card-title">{icon} {title}</div>
      <p className="ha-hint" style={{ margin: '2px 0 8px' }}>{t('hotelsAnalytics.clickHint')}</p>
      <div className="ha-table-wrap">
        <table className="ha-table">
          <thead>
            <tr>
              <th>{t('hotelsAnalytics.hotel')}</th>
              <th>{t('hotelsAnalytics.city')}</th>
              <th>{t('hotelsAnalytics.country')}</th>
              <th className="ha-stars-col">{t('hotelsAnalytics.stars')}</th>
              <th className="ha-num">{valueLabel}</th>
            </tr>
          </thead>
          <tbody>
            {items.map((h) => {
              const n = Math.min(5, Math.max(0, Number(h.starRating) || 0));
              const isExpanded = expandedHotelId === h.hotelId;
              return (
              <Fragment key={h.hotelId}>
              <tr
                className={isExpanded ? 'ha-row-active' : ''}
                onClick={() => setExpandedHotelId(isExpanded ? null : h.hotelId)}
              >
                <td><strong>{h.hotelName}</strong></td>
                <td>{h.city}</td>
                <td>{h.country}</td>
                <td className="ha-stars-col">
                  <span className="ha-star-rating">
                    <span className="ha-star-filled">{'★'.repeat(n)}</span>
                    <span className="ha-star-empty">{'★'.repeat(5 - n)}</span>
                  </span>
                </td>
                <td className="ha-num">{formatValue(h[valueKey])}</td>
              </tr>
              {isExpanded && <HotelRoomsRow hotelId={h.hotelId} />}
              </Fragment>
              );
            })}
            {items.length === 0 && (
              <tr><td colSpan={5} className="ha-hint">{t('hotelsAnalytics.noHotelsYet')}</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default function HotelsAnalytics() {
  const { t } = useTranslation();
  const now = new Date();
  const [mode, setMode] = useState('all'); // 'all' | 'year' | 'month'
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);

  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    setError('');
    const filters = mode === 'all' ? {} : mode === 'year' ? { year } : { year, month };
    getAdminDashboard(filters)
      .then((d) => { if (mounted) setData(d); })
      .catch((err) => { if (mounted) setError(err.message || t('hotelsAnalytics.loadError')); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [mode, year, month, t]);

  const filterBar = <PeriodFilter mode={mode} setMode={setMode} year={year} setYear={setYear} month={month} setMonth={setMonth} />;

  if (loading) return <div className="ha-root">{filterBar}<p className="admin-stat-sub">{t('hotelsAnalytics.loading')}</p></div>;
  if (error) return <div className="ha-root">{filterBar}<p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p></div>;
  if (!data) return null;

  const { revenue, bookingStats, topHotelsByRevenue, topHotelsByBookings } = {
    revenue: data.revenue,
    bookingStats: data.bookingStats,
    topHotelsByRevenue: data.topHotelsByRevenue || [],
    topHotelsByBookings: data.topHotelsByBookings || [],
  };

  return (
    <div className="ha-root">
      {filterBar}

      {/* Summary stat cards */}
      <div className="admin-stats-row">
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('hotelsAnalytics.platformRevenue')}</div>
          <div className="admin-stat-value" style={{ fontSize: 20 }}>{formatMoney(revenue.totalRevenue)}</div>
          <div className="admin-stat-sub">{t('hotelsAnalytics.platformRevenueSub')}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('hotelsAnalytics.bookingFees')}</div>
          <div className="admin-stat-value" style={{ fontSize: 20 }}>{formatMoney(revenue.totalPlatformRevenue)}</div>
          <div className="admin-stat-sub">{t('hotelsAnalytics.bookingFeesSub')}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('hotelsAnalytics.cancellationRevenue')}</div>
          <div className="admin-stat-value" style={{ fontSize: 20 }}>{formatMoney(revenue.totalCancellationRevenue)}</div>
          <div className="admin-stat-sub">{t('hotelsAnalytics.cancellationRevenueSub')}</div>
        </div>
        <div className="admin-stat-card">
          <div className="admin-stat-label">{t('hotelsAnalytics.hotelsUsers')}</div>
          <div className="admin-stat-value" style={{ fontSize: 20 }}>{bookingStats.totalHotels} / {bookingStats.totalUsers}</div>
          <div className="admin-stat-sub">{t('hotelsAnalytics.totalRegistered')}</div>
        </div>
      </div>

      <div className="admin-card">
        <div className="admin-card-title">{t('hotelsAnalytics.bookingsByStatus')}</div>
        <BarChart
          items={[
            { key: t('myBookings.statuses.confirmed'), value: bookingStats.confirmedBookings },
            { key: t('myBookings.statuses.pending'), value: bookingStats.pendingBookings },
            { key: t('myBookings.statuses.completed'), value: bookingStats.completedBookings },
            { key: t('myBookings.statuses.cancelled'), value: bookingStats.cancelledBookings },
          ]}
          valueKey="value"
          labelKey="key"
          color="#6C8BC7"
        />
        <p className="ha-hint">{t('hotelsAnalytics.bookingsTotal', { count: bookingStats.totalBookings })}</p>
      </div>

      <RankingTable
        title={t('hotelsAnalytics.topHotelsByRevenue')}
        icon="💰"
        items={topHotelsByRevenue}
        valueKey="grossRevenue"
        valueLabel={t('hotelsAnalytics.grossRevenue')}
        formatValue={formatMoney}
      />

      <RankingTable
        title={t('hotelsAnalytics.topHotelsByBookings')}
        icon="📊"
        items={topHotelsByBookings}
        valueKey="bookingsCount"
        valueLabel={t('hotelsAnalytics.bookings')}
        formatValue={(v) => v}
      />
    </div>
  );
}
