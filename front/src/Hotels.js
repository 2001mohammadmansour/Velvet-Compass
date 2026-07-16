import React, { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import './room.css'
import * as hotelsSvc from './services/hotels'
const SYRIA_CITIES = [
  'Damascus', 'Aleppo', 'Homs', 'Hama', 'Latakia', 'Tartous',
  'Idlib', 'Palmyra', 'Bloudan', 'Deir ez-Zor', 'Qamishli',
  'Daraa', 'As-Suwayda', 'Raqqa', 'Douma', 'Quneitra',
]

const STAR_OPTIONS = [5, 4, 3, 2, 1]

export default function Hotels() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()

  const [hotelsData, setHotelsData] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [filters, setFilters] = useState({
    hotel: '',
    city: location.state?.initialFilters?.city || '',
    stars: location.state?.initialFilters?.stars ?? [],
  })

  useEffect(() => {
    let mounted = true
    setLoading(true)
    setError('')
    hotelsSvc.getHotels()
      .then((data) => { if (mounted) setHotelsData(Array.isArray(data) ? data : []) })
      .catch((err) => { if (mounted) setError(err.message || t('hotels.loadError')) })
      .finally(() => { if (mounted) setLoading(false) })
    return () => { mounted = false }
  }, [t])

  const toggleStar = (s) => {
    setFilters(f => ({
      ...f,
      stars: f.stars.includes(s) ? f.stars.filter(x => x !== s) : [...f.stars, s],
    }))
  }

  const clearFilters = () => setFilters({ hotel: '', city: '', stars: [] })

  const activeFilterCount =
    (filters.hotel ? 1 : 0) + (filters.city ? 1 : 0) + filters.stars.length

  const results = hotelsData.filter(h => {
    const nameOk = !filters.hotel || String(h.hotelName || '').toLowerCase().includes(filters.hotel.toLowerCase())
    const cityOk = !filters.city || String(h.city || '').toLowerCase() === filters.city.toLowerCase()
    const starsOk = filters.stars.length === 0 || filters.stars.includes(Number(h.stars))
    return nameOk && cityOk && starsOk
  })

  const handleSelect = (hotel) => {
    navigate('/rooms', { state: { selectedHotel: hotel.hotelName, hotel } })
  }

  return (
    <div className="sr-page">
      {/* Top bar */}
      <div className="sr-topbar">
        <div className="sr-topbar-center">
          <h1 className="sr-title">{t('hotels.title')}</h1>
        </div>
        <input
          className="sr-filter-input"
          style={{ width: 220 }}
          placeholder={t('hotels.searchPlaceholder')}
          value={filters.hotel}
          onChange={e => setFilters(f => ({ ...f, hotel: e.target.value }))}
        />
        {activeFilterCount > 0 && (
          <button className="sr-clear-btn" onClick={clearFilters}>
            {t('common.clearAll')} <span className="sr-filter-badge">{activeFilterCount}</span>
          </button>
        )}
      </div>

      {loading && <p className="sr-status">{t('hotels.loading')}</p>}
      {error && <p className="sr-status sr-error">{error}</p>}

      <div className="sr-body">
        {/* Sidebar filters */}
        <aside className="sr-sidebar">
          <div className="sr-sidebar-header">
            <span className="sr-sidebar-title">{t('common.filters')}</span>
            {activeFilterCount > 0 && (
              <button className="sr-clear-btn" onClick={clearFilters}>{t('common.clearAll')}</button>
            )}
          </div>

          {/* City */}
          <div className="sr-filter-section">
            <span className="sr-filter-label">{t('common.city')}</span>
            <select
              className="sr-filter-input"
              value={filters.city}
              onChange={e => setFilters(f => ({ ...f, city: e.target.value }))}
            >
              <option value="">{t('common.allCities')}</option>
              {SYRIA_CITIES.map(c => (
                <option key={c} value={c}>{c}</option>
              ))}
            </select>
          </div>

          {/* Stars */}
          <div className="sr-filter-section">
            <span className="sr-filter-label">{t('common.stars')}</span>
            <div className="sr-star-row">
              {STAR_OPTIONS.map(s => (
                <button
                  key={s}
                  className={`sr-star-btn${filters.stars.includes(s) ? ' active' : ''}`}
                  onClick={() => toggleStar(s)}
                >
                  {'★'.repeat(s)}
                </button>
              ))}
            </div>
          </div>
        </aside>

        {/* Results */}
        <main className="sr-results">
          {!loading && (
            <p className="sr-count">
              {results.length} {results.length === 1 ? t('hotels.hotel') : t('hotels.hotelsPlural')} {t('hotels.found')}
            </p>
          )}

          {!loading && results.length === 0 && (
            <div className="sr-empty">
              <p style={{ fontSize: 16, color: '#6b7280', marginBottom: 8 }}>{t('hotels.noMatch')}</p>
              <button className="sr-clear-btn-lg" onClick={clearFilters}>{t('hotels.clearFilters')}</button>
            </div>
          )}

          <div className="sr-grid">
            {results.map((h, i) => (
              <div
                key={h.hotelId || h.hotelName || i}
                className="sr-card"
                onClick={() => handleSelect(h)}
                style={{ cursor: 'pointer', animationDelay: `${i * 0.07}s` }}
              >
                {h.cardPhoto
                  ? <img className="sr-card-img" src={h.cardPhoto} alt={h.hotelName} />
                  : <div className="sr-card-img-placeholder" />
                }
                <div className="sr-card-body">
                  <h3 className="sr-card-name">{h.hotelName || t('hotels.defaultHotelName')}</h3>
                  <p className="sr-card-hotel">📍 {h.city || t('hotels.cityNotSet')}</p>
                  {h.stars ? (
                    <p className="sr-card-stars">
                      {'★'.repeat(Number(h.stars))}{'☆'.repeat(5 - Number(h.stars))}
                    </p>
                  ) : null}
                  <div className="sr-card-footer">
                    <p className="sr-card-price">
                      {typeof h.minPrice === 'number' && typeof h.maxPrice === 'number' ? (
                        <>
                          <span className="sr-price-amount">${h.minPrice}–${h.maxPrice}</span>
                          <span className="sr-price-night">{t('hotels.perNight')}</span>
                        </>
                      ) : (
                        <span className="sr-price-night">{t('hotels.pricingNotSet')}</span>
                      )}
                    </p>
                    <button
                      className="sr-book-btn"
                      onClick={e => { e.stopPropagation(); handleSelect(h) }}
                    >
                      {t('hotels.viewRooms')}
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </main>
      </div>
    </div>
  )
}
