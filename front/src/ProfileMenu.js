import { Link } from 'react-router-dom';
import './home.css';
import NotificationBell from './NotificationBell';

// CHANGED BY AI (2026-07-13): new shared component — previously Navbar.js and OwnerHome.js each
// had their own near-identical copy of this profile trigger/dropdown, which is exactly how the
// hotel_owner role ended up missing from Navbar.js in the first place (it was built once for
// guests, and the owner version was written separately in OwnerHome.js and never kept in sync).
// One shared component now backs every place a logged-in user sees their profile menu.
export default function ProfileMenu({ name, subtitle, links, onSignOut, triggerLabel = 'Account profile' }) {
  return (
    <div className="owner-profile-slot">
      <NotificationBell inline />
      <div className="owner-profile-menu" tabIndex={0}>
        {/* CHANGED BY AI (2026-07-13): please review — trigger no longer repeats the name (it
            was showing once here and again in the dropdown header right below it); just the icon
            and a caret now, name lives in one place only. */}
        <button className="owner-profile-trigger" type="button" aria-label={triggerLabel}>
          <span className="owner-profile-icon">👤</span>
          <span className="owner-profile-caret">▾</span>
        </button>
        <div className="owner-profile-dropdown">
          <div className="owner-profile-header">
            <span className="owner-profile-icon owner-profile-icon-lg">👤</span>
            <div>
              <p className="owner-profile-name-big">{name}</p>
              {subtitle && <p className="owner-profile-email">{subtitle}</p>}
            </div>
          </div>
          <div className="owner-profile-divider" />
          {links.map((link) => (
            <Link key={link.to} to={link.to} className="owner-profile-dashboard-link">{link.label}</Link>
          ))}
          <div className="owner-profile-divider" />
          <button type="button" className="owner-profile-signout-btn" onClick={onSignOut}>Sign Out</button>
        </div>
      </div>
    </div>
  );
}
