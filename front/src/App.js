import { Routes, Route, Navigate, useLocation } from "react-router-dom";
import { getCurrentRole } from "./services/auth";
import './App.css';
import './animations.css';
import Home from "./home";
import Rooms from "./Rooms";
import RoomDetail from "./RoomDetail";
import Hotels from "./Hotels";
import SearchResults from "./SearchResults";
import MyBookings from "./MyBookings";
import Reservation from "./Reservation"
import FacilitiesAttractions from "./FacilitiesAttractions";
import SignUp from "./SignUp";
import ServicesSection from "./ServicesSection"
import Login from "./Login"
import OwnerDashboard from "./OwnerDashboard";
import OwnerStats from "./OwnerStats";
import OwnerHotelInfo from "./OwnerHotelInfo";
import OwnerRequests from "./OwnerRequests";
import AboutUs from "./AboutUs"
import Contact from "./Contact"
import AllCities from "./AllCities";
import AdminDashboard from "./AdminDashboard";
import ScrollToTop from "./ScrollToTop";
import Navbar from "./Navbar";
import NotificationBell from "./NotificationBell";
import EditProfile from "./EditProfile";

const PUBLIC_NAV = new Set([
  '/', '/home', '/hotels', '/rooms', '/room-detail', '/search', '/my-bookings',
  '/reservation', '/services', '/about', '/contact', '/cities',
  '/facilities-attractions', '/profile',
]);

function OwnerRoute({ children }) {
  if (getCurrentRole() !== "hotel_owner") return <Navigate to="/login" replace />;
  return children;
}

function AdminRoute({ children }) {
  if (getCurrentRole() !== "admin") return <Navigate to="/login" replace />;
  return children;
}

export default function App() {
  const location = useLocation();
  const isHome = location.pathname === '/' || location.pathname === '/home';
  const showNavbar = PUBLIC_NAV.has(location.pathname);

  return (
    <>
      <ScrollToTop />
      {/* CHANGED BY AI (2026-07-13): please review — owners no longer have a separate home page
          (see the /ownerhome redirect below), so every page with a shared Navbar already covers
          them; the floating fallback bell is only needed on pages without one. */}
      {!showNavbar && <NotificationBell />}
      {showNavbar && <Navbar transparent={isHome} />}
      <div key={location.pathname} className="page-enter">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/home" element={<Home />} />
          {/* CHANGED BY AI (2026-07-13): please review — removed the separate owner home page
              per request; both /owner and /ownerhome now just redirect to the normal home page.
              Kept as redirects (rather than deleted routes) so existing links elsewhere in the
              app (OwnerDashboard.js, OwnerHotelInfo.js, OwnerRequests.js, etc.) keep working. */}
          <Route path="/owner" element={<Navigate to="/" replace />} />
          <Route path="/ownerhome" element={<Navigate to="/" replace />} />
          <Route path="/rooms" element={<Rooms />} />
          {/* CHANGED BY AI (2026-07-15): please review — new room detail/product page between the
              room list and checkout; see RoomDetail.js. */}
          <Route path="/room-detail" element={<RoomDetail />} />
          <Route path="/hotels" element={<Hotels />} />
          <Route path="/search" element={<SearchResults />} />
          <Route path="/my-bookings" element={<MyBookings />} />
          <Route path="/profile" element={<EditProfile />} />
          <Route path="/facilities-attractions" element={<FacilitiesAttractions />} />
          <Route path="/reservation" element={<Reservation />} />
          <Route path="/signup" element={<SignUp />} />
          <Route path="/services" element={<ServicesSection />} />
          <Route path="/about" element={<AboutUs />} />
          <Route path="/contact" element={<Contact />} />
          <Route path="/cities" element={<AllCities />} />
          <Route path="/login" element={<Login />} />
          <Route path="/owner/dashboard" element={<OwnerRoute><OwnerDashboard /></OwnerRoute>} />
          <Route path="/owner/stats" element={<OwnerRoute><OwnerStats /></OwnerRoute>} />
          <Route path="/owner/hotel-info" element={<OwnerRoute><OwnerHotelInfo /></OwnerRoute>} />
          <Route path="/owner/requests" element={<OwnerRequests />} />
          <Route path="/admin" element={<AdminRoute><AdminDashboard /></AdminRoute>} />
        </Routes>
      </div>
    </>
  );
}
