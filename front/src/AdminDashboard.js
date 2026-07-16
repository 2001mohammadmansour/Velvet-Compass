import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import HotelsAnalytics from './HotelsAnalytics';
import HotelRequests from './HotelRequests';
import AdminStats from './AdminStats';
import AdminUsers from './AdminUsers';
import AmenitiesAdmin from './AmenitiesAdmin';
import { getAdminDashboard } from './services/hotels';
import { getAllHotelRequests } from './services/hotelRequests';
import './AdminDashboard.css';

function formatMoney(value) {
  return `$${Math.round(Number(value) || 0).toLocaleString()}`;
}

/* ── Overview Tab ── */
// CHANGED: this used to just repeat the sidebar's nav as clickable cards with no actual data.
// Now leads with real platform-wide numbers (reusing the same admin/dashboard + hotel-requests
// data the Hotels Analytics / Hotel Requests tabs already fetch), so it's an actual at-a-glance
// summary instead of a second nav menu.
function OverviewTab({ onTabChange }) {
  const { t } = useTranslation();
  const [data, setData] = useState(null);
  const [pendingRequests, setPendingRequests] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let mounted = true;
    Promise.all([getAdminDashboard(), getAllHotelRequests()])
      .then(([dashboard, requests]) => {
        if (!mounted) return;
        setData(dashboard);
        setPendingRequests(requests.filter((r) => r.status === 'pending').length);
      })
      .catch((err) => { if (mounted) setError(err.message || t('adminDashboard.overviewTab.loadError')); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, [t]);

  return (
    <>
      {loading && <p className="admin-stat-sub">{t('adminDashboard.overviewTab.loading')}</p>}
      {error && <p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p>}

      {data && (
        <div className="admin-stats-row">
          <div className="admin-stat-card">
            <div className="admin-stat-label">{t('adminDashboard.overviewTab.platformRevenue')}</div>
            <div className="admin-stat-value" style={{ fontSize: 20 }}>{formatMoney(data.revenue?.totalRevenue)}</div>
            <div className="admin-stat-sub">{t('adminDashboard.overviewTab.platformRevenueSub')}</div>
          </div>
          <div className="admin-stat-card">
            <div className="admin-stat-label">{t('adminDashboard.overviewTab.hotelsUsers')}</div>
            <div className="admin-stat-value" style={{ fontSize: 20 }}>{data.bookingStats?.totalHotels ?? 0} / {data.bookingStats?.totalUsers ?? 0}</div>
            <div className="admin-stat-sub">{t('adminDashboard.overviewTab.totalRegistered')}</div>
          </div>
          <div className="admin-stat-card">
            <div className="admin-stat-label">{t('adminDashboard.overviewTab.confirmedBookings')}</div>
            <div className="admin-stat-value" style={{ fontSize: 20 }}>{data.bookingStats?.confirmedBookings ?? 0}</div>
            <div className="admin-stat-sub">{t('adminDashboard.overviewTab.allTime')}</div>
          </div>
          <div
            className="admin-stat-card"
            style={{ cursor: 'pointer' }}
            onClick={() => onTabChange('requests')}
          >
            <div className="admin-stat-label">{t('adminDashboard.overviewTab.pendingHotelRequests')}</div>
            <div className="admin-stat-value" style={{ fontSize: 20, color: pendingRequests > 0 ? '#f59e0b' : undefined }}>
              {pendingRequests}
            </div>
            <div className="admin-stat-sub">{pendingRequests > 0 ? t('adminDashboard.overviewTab.needsReview') : t('adminDashboard.overviewTab.allCaughtUp')}</div>
          </div>
        </div>
      )}
    </>
  );
}

/* ══════════════════════════════════
   MAIN ADMIN DASHBOARD COMPONENT
══════════════════════════════════ */
export default function AdminDashboard() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState('overview');

  const NAV_ITEMS = [
    { key: 'overview', icon: '📊', label: t('adminDashboard.nav.overview') },
    { key: 'hotels', icon: '🏨', label: t('adminDashboard.nav.hotels') },
    { key: 'stats', icon: '📈', label: t('adminDashboard.nav.stats') },
    { key: 'users', icon: '👥', label: t('adminDashboard.nav.users') },
    { key: 'requests', icon: '📥', label: t('adminDashboard.nav.requests') },
    { key: 'amenities', icon: '🛎️', label: t('adminDashboard.nav.amenities') },
  ];

  const SECTION_DESCRIPTIONS = {
    overview: t('adminDashboard.sectionDescriptions.overview'),
    hotels: t('adminDashboard.sectionDescriptions.hotels'),
    stats: t('adminDashboard.sectionDescriptions.stats'),
    users: t('adminDashboard.sectionDescriptions.users'),
    requests: t('adminDashboard.sectionDescriptions.requests'),
    amenities: t('adminDashboard.sectionDescriptions.amenities'),
  };

  return (
    <div className="admin-root">
      <div className="admin-body">
        {/* Sidebar */}
        <aside className="admin-sidebar">
          <div className="admin-sidebar-label">{t('adminDashboard.navigation')}</div>
          {NAV_ITEMS.map((item) => (
            <div
              key={item.key}
              className={`admin-nav-item ${activeTab === item.key ? 'active' : ''}`}
              onClick={() => setActiveTab(item.key)}
            >
              <span className="admin-nav-icon">{item.icon}</span>
              {item.label}
            </div>
          ))}
        </aside>

        {/* Main */}
        <main className="admin-main">
          <div className="admin-section-header">
            <h2>
              {NAV_ITEMS.find((n) => n.key === activeTab)?.icon}{' '}
              {NAV_ITEMS.find((n) => n.key === activeTab)?.label}
            </h2>
            <p>{SECTION_DESCRIPTIONS[activeTab]}</p>
          </div>

          {activeTab === 'overview' && <OverviewTab onTabChange={setActiveTab} />}
          {activeTab === 'hotels' && <HotelsAnalytics />}
          {activeTab === 'stats' && <AdminStats />}
          {activeTab === 'users' && <AdminUsers />}
          {activeTab === 'requests' && <HotelRequests />}
          {activeTab === 'amenities' && <AmenitiesAdmin />}
        </main>
      </div>
    </div>
  );
}
