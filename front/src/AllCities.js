import './home.css';
import './AllCities.css';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import damascusImg from './assets/Damascus.jpg';
import aleppoImg   from './assets/Aleppo.jpg';
import tartousImg  from './assets/Tartous.jpg';
import latakiaImg  from './assets/Latakia.jpg';
import hamaImg     from './assets/hama.jpg';
import homsImg     from './assets/homs.jpg';
import idlibImg    from './assets/idlib.jpg';
import palmyraImg  from './assets/palmyra.jpg';
import bloudanImg  from './assets/bloudan.jpg';
import deirImg     from './assets/deir.jpg';
import qamishliImg from './assets/qamshly.jpg';
import daraaImg    from './assets/daraa.jpg';
import suwaydaImg  from './assets/assuwda.jpg';
import raqqaImg    from './assets/raqqa.jpg';
import doumaImg    from './assets/douma.jpg';
import quneitraImg from './assets/quneitra.jpg';


const ALL_CITIES = [
  { name: 'Damascus',    img: damascusImg },
  { name: 'Aleppo',      img: aleppoImg },
  { name: 'Homs',        img: homsImg },
  { name: 'Hama',        img: hamaImg },
  { name: 'Latakia',     img: latakiaImg },
  { name: 'Tartous',     img: tartousImg },
  { name: 'Deir ez-Zor', img: deirImg },
  { name: 'Idlib',       img: idlibImg },
  { name: 'Qamishli',    img: qamishliImg },
  { name: 'Daraa',       img: daraaImg },
  { name: 'As-Suwayda',  img: suwaydaImg },
  { name: 'Palmyra',     img: palmyraImg },
  { name: 'Raqqa',       img: raqqaImg },
  { name: 'Douma',       img: doumaImg },
  { name: 'Bloudan',     img: bloudanImg },
  { name: 'Quneitra',    img: quneitraImg },
];

export default function AllCities() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <div className="home">
      <div className="ac-hero">
        <h1 className="ac-hero-title">{t('allCities.title')}</h1>
        <p className="ac-hero-sub">{t('allCities.subtitle')}</p>
      </div>

      <div className="ac-grid-section">
        <div className="ac-grid">
          {ALL_CITIES.filter(c => !c.hidden).map((city, i) => (
            <div
              key={city.name}
              className="ac-card"
              style={{ animationDelay: `${i * 0.06}s` }}
              onClick={() => navigate('/hotels', { state: { initialFilters: { city: city.name } } })}
            >
              <div className="ac-img-placeholder">
                {city.img
                  ? <img src={city.img} alt={city.name} />
                  : <span className="ac-city-icon">🏙️</span>
                }
              </div>
              <div className="ac-card-body">
                <h3 className="ac-city-name">{city.name}</h3>
                <p className="ac-city-desc">{t(`allCities.descriptions.${city.name}`)}</p>
                <button className="ac-explore-btn">{t('allCities.exploreHotels')}</button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <footer className="footer">
        <p>{t('allCities.footer')}</p>
      </footer>
    </div>
  );
}
