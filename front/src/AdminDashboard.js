import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import HotelsAnalytics from './HotelsAnalytics';
import HotelRequests from './HotelRequests';
import AdminStats from './AdminStats';
import AdminUsers from './AdminUsers';
import AmenitiesAdmin from './AmenitiesAdmin';
import NotificationBell from './NotificationBell';
import { clearAuth } from './services/auth';
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
      .catch((err) => { if (mounted) setError(err.message || 'Unable to load overview.'); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, []);

  return (
    <>
      {loading && <p className="admin-stat-sub">Loading overview...</p>}
      {error && <p className="admin-stat-sub" style={{ color: '#e05555' }}>{error}</p>}

      {data && (
        <div className="admin-stats-row">
          <div className="admin-stat-card">
            <div className="admin-stat-label">Platform Revenue</div>
            <div className="admin-stat-value" style={{ fontSize: 20 }}>{formatMoney(data.revenue?.totalRevenue)}</div>
            <div className="admin-stat-sub">All-time booking fees + penalties</div>
          </div>
          <div className="admin-stat-card">
            <div className="admin-stat-label">Hotels / Users</div>
            <div className="admin-stat-value" style={{ fontSize: 20 }}>{data.bookingStats?.totalHotels ?? 0} / {data.bookingStats?.totalUsers ?? 0}</div>
            <div className="admin-stat-sub">Total registered</div>
          </div>
          <div className="admin-stat-card">
            <div className="admin-stat-label">Confirmed Bookings</div>
            <div className="admin-stat-value" style={{ fontSize: 20 }}>{data.bookingStats?.confirmedBookings ?? 0}</div>
            <div className="admin-stat-sub">All-time</div>
          </div>
          <div
            className="admin-stat-card"
            style={{ cursor: 'pointer' }}
            onClick={() => onTabChange('requests')}
          >
            <div className="admin-stat-label">Pending Hotel Requests</div>
            <div className="admin-stat-value" style={{ fontSize: 20, color: pendingRequests > 0 ? '#f59e0b' : undefined }}>
              {pendingRequests}
            </div>
            <div className="admin-stat-sub">{pendingRequests > 0 ? 'Needs review' : 'All caught up'}</div>
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
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');

  const handleLogout = () => {
    clearAuth();
    navigate('/login');
  };

  const NAV_ITEMS = [
    { key: 'overview', icon: '📊', label: 'Overview' },
    { key: 'hotels', icon: '🏨', label: 'Hotels Analytics' },
    { key: 'stats', icon: '📈', label: 'Revenue Stats' },
    { key: 'users', icon: '👥', label: 'Users' },
    { key: 'requests', icon: '📥', label: 'Hotel Requests' },
    { key: 'amenities', icon: '🛎️', label: 'Amenities' },
  ];

  const SECTION_DESCRIPTIONS = {
    overview: 'Admin panel overview and quick navigation',
    hotels: 'Platform revenue, bookings, and top hotels',
    stats: 'Monthly, quarterly, and yearly revenue charts across all hotels',
    users: 'All users, their booking activity, and hotels owned',
    requests: 'Approve or reject hotel owner requests to create or edit a hotel',
    amenities: 'Manage the catalog of hotel and room benefits owners can pick from',
  };

  return (
    <div className="admin-root">
      {/* Header */}
      <header className="admin-header">
        <div className="admin-header-brand">
          Velvet Compass <span>Admin Panel</span>
        </div>
        <div className="admin-header-actions">
          <div className="admin-preview-live">
            <div className="admin-pulse" />
            Live Site
          </div>
          <Link to="/" target="_blank" className="admin-header-btn admin-header-btn-outline">
            ↗ View Site
          </Link>
          <button className="admin-header-btn admin-header-btn-danger" onClick={handleLogout}>
            Sign Out
          </button>
          <NotificationBell inline />
        </div>
      </header>

      <div className="admin-body">
        {/* Sidebar */}
        <aside className="admin-sidebar">
          <div className="admin-sidebar-label">Navigation</div>
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
