import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import "./signUp.css";
import { signUpUser } from "./services/auth";
import { submitHotelRequest } from "./services/hotelRequests";
import { fileToResizedDataUrl } from "./data/imageUtil";
import LanguageToggle from "./LanguageToggle";

const initialForm = {
  username: "",
  email: "",
  phoneNumber: "",
  hotelName: "",
  city: "",
  stars: 0,
  password: "",
  confirmPassword: "",
  acceptTerms: false,
};

function StarPicker({ value, onChange }) {
  return (
    <div style={{ display: 'flex', gap: 6 }}>
      {[1, 2, 3, 4, 5].map((n) => (
        <span
          key={n}
          onClick={() => onChange(n)}
          style={{ fontSize: 32, cursor: 'pointer', color: n <= value ? '#f59e0b' : '#d1d5db', userSelect: 'none', lineHeight: 1 }}
        >
          ★
        </span>
      ))}
    </div>
  );
}

const SYRIAN_CITIES = [
  'Damascus', 'Aleppo', 'Homs', 'Hama', 'Latakia', 'Tartous',
  'Idlib', 'Palmyra', 'Bloudan', 'Deir ez-Zor', 'Qamishli',
  'Daraa', 'As-Suwayda', 'Raqqa', 'Douma', 'Quneitra',
];

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phoneRegex = /^\+?[0-9\s\-()]{7,20}$/;
// At least 8 chars, 1 uppercase, 1 lowercase, 1 special character
const passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*[^A-Za-z0-9]).{8,}$/;

const LEGAL_CONTENT = {
  terms: {
    title: "Terms of Service",
    paragraphs: [
      "Welcome to Velvet Compass. By creating an account and using our booking platform, you agree to use the service only for lawful purposes and to provide accurate information when registering or making a reservation.",
      "Bookings made through Velvet Compass are subject to each hotel's individual cancellation and payment policies, which will be displayed before you confirm a reservation. Velvet Compass acts as a marketplace connecting guests and hotel owners, and is not itself responsible for the condition or service quality of any listed property.",
      "Hotel owners are responsible for keeping their listings, pricing, and availability accurate. Misuse of the platform, fraudulent listings, or fraudulent bookings may result in suspension of an account at our discretion.",
      "We may update these terms from time to time. Continued use of Velvet Compass after changes are posted constitutes acceptance of the revised terms.",
    ],
  },
  privacy: {
    title: "Privacy Policy",
    paragraphs: [
      "Velvet Compass collects the information you provide when signing up, such as your name, email address, phone number, and — for hotel owners — your property details. This information is used to create and manage your account, process bookings, and communicate with you about your reservations.",
      "We do not sell your personal information to third parties. Limited data may be shared with hotel owners solely to facilitate a reservation you make on their property.",
      "We use industry-standard practices to protect your data, including secure password storage. You may request access to, correction of, or deletion of your personal data at any time by contacting our support team.",
      "Cookies and similar technologies may be used to keep you signed in and to improve your experience on the site. You can control cookie preferences through your browser settings.",
    ],
  },
};

function SignUp() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const [doc, setDoc] = useState(null); // { name, dataUrl } — hotel document image
  const [isOwnerSignUp, setIsOwnerSignUp] = useState(false);
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState("");
  const [loading, setLoading] = useState(false);
  const [legalModal, setLegalModal] = useState(null); // 'terms' | 'privacy' | null

  const passwordRules = useMemo(
    () => ({
      minLength: form.password.length >= 8,
      hasUpper: /[A-Z]/.test(form.password),
      hasLower: /[a-z]/.test(form.password),
      hasSpecial: /[^A-Za-z0-9]/.test(form.password),
    }),
    [form.password]
  );

  const normalizePhone = (value) => value.trim().replace(/\s+/g, " ");

  const validate = (data) => {
    const nextErrors = {};

    if (!data.username.trim()) nextErrors.username = t('auth.signup.errors.usernameRequired');
    if (!emailRegex.test(data.email.trim())) {
      nextErrors.email = t('auth.signup.errors.emailInvalid');
    }
    if (!phoneRegex.test(data.phoneNumber.trim())) {
      nextErrors.phoneNumber = t('auth.signup.errors.phoneInvalid');
    }
    if (isOwnerSignUp && !data.hotelName.trim()) {
      nextErrors.hotelName = t('auth.signup.errors.hotelNameRequired');
    }
    if (isOwnerSignUp && !data.city.trim()) {
      nextErrors.city = t('auth.signup.errors.cityRequired');
    }
    if (isOwnerSignUp && !data.stars) {
      nextErrors.stars = t('auth.signup.errors.starsRequired');
    }
    if (!passwordRegex.test(data.password)) {
      nextErrors.password = t('auth.signup.errors.passwordInvalid');
    }
    if (!data.confirmPassword) {
      nextErrors.confirmPassword = t('auth.signup.errors.confirmPasswordRequired');
    } else if (data.confirmPassword !== data.password) {
      nextErrors.confirmPassword = t('auth.signup.errors.passwordsDontMatch');
    }
    if (!data.acceptTerms) {
      nextErrors.acceptTerms = t('auth.signup.errors.acceptTermsRequired');
    }

    return nextErrors;
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setSubmitError("");
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleDocChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 8 * 1024 * 1024) {
      setErrors((prev) => ({ ...prev, document: t('auth.signup.errors.documentTooLarge') }));
      e.target.value = "";
      return;
    }
    try {
      const dataUrl = await fileToResizedDataUrl(file);
      setDoc({ name: file.name, dataUrl });
      setErrors((prev) => ({ ...prev, document: undefined }));
    } catch {
      setErrors((prev) => ({ ...prev, document: t('auth.signup.errors.documentUnreadable') }));
    }
    e.target.value = "";
  };

  const handleSignUpModeToggle = () => {
    setIsOwnerSignUp((prev) => !prev);
    setForm(initialForm);
    setDoc(null);
    setErrors({});
    setSubmitError("");
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const nextErrors = validate(form);
    if (isOwnerSignUp && !doc) {
      nextErrors.document = t('auth.signup.errors.documentRequired');
    }
    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) return;

    setLoading(true);
    setSubmitError("");

    const payload = {
      username: form.username.trim(),
      email: form.email.trim().toLowerCase(),
      phoneNumber: normalizePhone(form.phoneNumber),
      role: isOwnerSignUp ? "hotel_owner" : "guest",
      password: form.password,
      acceptTerms: form.acceptTerms,
    };
    let ownerRequest = null;
    if (isOwnerSignUp) {
      payload.hotelName = form.hotelName.trim();
      payload.city = form.city.trim();
      payload.stars = form.stars || null;
      ownerRequest = {
        hotelName: payload.hotelName,
        city: payload.city,
        phoneNumber: payload.phoneNumber,
        stars: form.stars || null,
        document: doc,
      };
    }

    try {
      // The backend's register endpoint already returns an auth session — no email verification.
      const res = await signUpUser(payload);

      const existingRaw = localStorage.getItem("mock_auth_user");
      const existing = existingRaw ? JSON.parse(existingRaw) : {};
      localStorage.setItem("mock_auth_user", JSON.stringify({
        ...res,
        user: {
          ...(existing?.user || {}),
          ...(res?.user || {}),
          hotelId: res?.user?.hotelId || existing?.user?.hotelId || null,
          hotelName: res?.user?.hotelName || existing?.user?.hotelName || null,
        },
      }));

      // Now that we have a real auth token, submit the hotel request. A failure here shouldn't
      // block the account — the owner can resubmit from their requests page.
      if (ownerRequest) {
        try {
          await submitHotelRequest({ type: "create", ...ownerRequest });
        } catch (reqErr) {
          alert(t('auth.signup.errors.hotelRequestSubmitFailedNotice'));
        }
      }

      navigate("/");
    } catch (err) {
      setSubmitError(err.message || t('auth.signup.errors.signUpFailed'));
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
        <h1>{isOwnerSignUp ? t('auth.signup.createOwnerAccount') : t('auth.signup.createAccount')}</h1>
        <p className="subtitle">
          {isOwnerSignUp ? t('auth.signup.subtitleOwner') : t('auth.signup.subtitleGuest')}
        </p>

        <form onSubmit={handleSubmit} noValidate>
          <label>
            {t('auth.signup.username')}
            <input
              name="username"
              type="text"
              value={form.username}
              onChange={handleChange}
              placeholder={t('auth.signup.usernamePlaceholder')}
              autoComplete="username"
            />
            {errors.username && <span className="error">{errors.username}</span>}
          </label>

          <label>
            {t('auth.signup.email')}
            <input
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              placeholder={t('auth.signup.emailPlaceholder')}
              autoComplete="email"
            />
            {errors.email && <span className="error">{errors.email}</span>}
          </label>

          <label>
            {t('auth.signup.phoneNumber')}
            <input
              name="phoneNumber"
              type="tel"
              value={form.phoneNumber}
              onChange={handleChange}
              placeholder="+963 912345678"
              autoComplete="tel"
            />
            {errors.phoneNumber && (
              <span className="error">{errors.phoneNumber}</span>
            )}
          </label>

          {isOwnerSignUp && (
            <>
              <label>
                {t('auth.signup.hotelName')}
                <input
                  name="hotelName"
                  type="text"
                  value={form.hotelName}
                  onChange={handleChange}
                  placeholder={t('auth.signup.hotelNamePlaceholder')}
                  autoComplete="organization"
                />
                {errors.hotelName && <span className="error">{errors.hotelName}</span>}
              </label>

              <label>
                {t('auth.signup.city')}
                <select
                  name="city"
                  value={form.city}
                  onChange={handleChange}
                >
                  <option value="">{t('auth.signup.selectCity')}</option>
                  {SYRIAN_CITIES.map((c) => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
                {errors.city && <span className="error">{errors.city}</span>}
              </label>

              <div>
                <label style={{ display: 'block', marginBottom: 6 }}>{t('auth.signup.starRating')}</label>
                <StarPicker
                  value={form.stars}
                  onChange={(n) => { setSubmitError(""); setForm((prev) => ({ ...prev, stars: n })); }}
                />
                {errors.stars && <span className="error">{errors.stars}</span>}
              </div>

              <label>
                {t('auth.signup.documentImage')}
                <input type="file" accept="image/*" onChange={handleDocChange} />
                {doc && (
                  <span className="signup-doc-preview">
                    <img src={doc.dataUrl} alt="Document" />
                    <span className="signup-doc-name">{doc.name}</span>
                  </span>
                )}
                {errors.document && <span className="error">{errors.document}</span>}
              </label>
            </>
          )}

          <label>
            {t('auth.signup.password')}
            <input
              name="password"
              type="password"
              value={form.password}
              onChange={handleChange}
              placeholder={t('auth.signup.passwordPlaceholder')}
              autoComplete="new-password"
            />
            {errors.password && <span className="error">{errors.password}</span>}
          </label>

          <ul className="password-hint">
            <li className={passwordRules.minLength ? "ok" : ""}>{t('auth.signup.passwordHints.minLength')}</li>
            <li className={passwordRules.hasUpper ? "ok" : ""}>{t('auth.signup.passwordHints.hasUpper')}</li>
            <li className={passwordRules.hasLower ? "ok" : ""}>{t('auth.signup.passwordHints.hasLower')}</li>
            <li className={passwordRules.hasSpecial ? "ok" : ""}>{t('auth.signup.passwordHints.hasSpecial')}</li>
          </ul>

          <label>
            {t('auth.signup.confirmPassword')}
            <input
              name="confirmPassword"
              type="password"
              value={form.confirmPassword}
              onChange={handleChange}
              placeholder={t('auth.signup.confirmPasswordPlaceholder')}
              autoComplete="new-password"
            />
            {errors.confirmPassword && (
              <span className="error">{errors.confirmPassword}</span>
            )}
          </label>

          <label className="checkbox-row">
            <input
              name="acceptTerms"
              type="checkbox"
              checked={form.acceptTerms}
              onChange={handleChange}
            />
            <span>
              {t('auth.signup.acceptPrefix')}{" "}
              <span
                className="legal-link"
                role="button"
                tabIndex={0}
                onClick={() => setLegalModal("terms")}
                onKeyDown={(e) => (e.key === "Enter" || e.key === " ") && setLegalModal("terms")}
              >
                {t('auth.signup.terms')}
              </span>{" "}
              {t('auth.signup.and')}{" "}
              <span
                className="legal-link"
                role="button"
                tabIndex={0}
                onClick={() => setLegalModal("privacy")}
                onKeyDown={(e) => (e.key === "Enter" || e.key === " ") && setLegalModal("privacy")}
              >
                {t('auth.signup.privacyPolicy')}
              </span>
              .
            </span>
          </label>
          {errors.acceptTerms && (
            <span className="error checkbox-error">{errors.acceptTerms}</span>
          )}

          <button type="submit" disabled={loading}>
            {loading ? t('auth.signup.signingUp') : t('auth.signup.signUp')}
          </button>
          <button
            type="button"
            className="link-button"
            onClick={handleSignUpModeToggle}
          >
            {isOwnerSignUp ? t('auth.signup.backToRegular') : t('auth.signup.signUpAsOwner')}
          </button>

          {submitError && <p className="error submit-error">{submitError}</p>}
        </form>
      </main>

      {legalModal && (
        <div className="legal-modal-overlay" onClick={() => setLegalModal(null)}>
          <div className="legal-modal" onClick={(e) => e.stopPropagation()}>
            <div className="legal-modal-header">
              <h2>{LEGAL_CONTENT[legalModal].title}</h2>
              <button type="button" className="legal-modal-close" onClick={() => setLegalModal(null)}>
                ✕
              </button>
            </div>
            <div className="legal-modal-body">
              {LEGAL_CONTENT[legalModal].paragraphs.map((p, i) => (
                <p key={i}>{p}</p>
              ))}
            </div>
            <button type="button" className="legal-modal-done" onClick={() => setLegalModal(null)}>
              {t('common.close')}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default SignUp;
