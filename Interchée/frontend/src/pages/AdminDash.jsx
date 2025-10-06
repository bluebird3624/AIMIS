import { useState, useEffect } from "react";
import '../Styles/dashboards.css'
import { useNavigate } from "react-router-dom";
import { motion } from 'framer-motion';


function AdminDash() {


    const [isMenuOpen, setIsMenuOpen] = useState(false);

    const openMenu = () => setIsMenuOpen(true);
    const closeMenu = () => setIsMenuOpen(false);

    
   
    return(
        <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '-100%' }}
            transition={{ type : 'tween' , duration: 2.1  }}
            className="absolute inset-0"
        
        >

        <div className="dash-body"    style={{ backgroundImage: "url('/src/assets/dashboards.png')"}}>
          <button className="menu-button" onClick={openMenu}>
            <span className="material-symbols-rounded">
                <svg xmlns="http://www.w3.org/2000/svg" height="48px" viewBox="0 -960 960 960" width="48px" fill="#167aa1"><path d="M142-203q-19.75 0-33.37-13.68Q95-230.35 95-251.18q0-19.82 13.63-33.32Q122.25-298 142-298h676q19.75 0 33.88 13.68Q866-270.65 866-250.82q0 20.82-14.12 34.32Q837.75-203 818-203H142Zm0-230q-19.75 0-33.37-13.68Q95-460.35 95-480.18q0-19.82 13.63-33.32Q122.25-527 142-527h676q19.75 0 33.88 13.68Q866-499.65 866-479.82q0 19.82-14.12 33.32Q837.75-433 818-433H142Zm0-229q-19.75 0-33.37-13.68Q95-689.35 95-710.18q0-19.82 13.63-33.32Q122.25-757 142-757h676q19.75 0 33.88 13.68Q866-729.65 866-709.82q0 20.82-14.12 34.32Q837.75-662 818-662H142Z"/></svg>
            </span>
          </button>
          {/* Menu Overlay */}
            <div 
        className={`menu-overlay ${isMenuOpen ? 'active' : ''}`}
        onClick={closeMenu}
      ></div>

      {/* Slide-in Menu */}
      <div className={`slide-in-menu ${isMenuOpen ? 'active' : ''}`}>
        <button className="menu-close" onClick={closeMenu}>×</button>
        <div className="menu-content">
          <h2 style={{ fontFamily : 'arial' , color : 'white'}}> ADMIN </h2>
          <button className="menu-options"> 
            Profile 
          </button>
          <button className="menu-options">
             Attachés / Interns 
            </button>
          <button className="menu-options"> Manage Users </button>
          <button className="menu-options"> Notifications </button>
          <button className="menu-options"> Log Out </button>
          <button className="menu-options"> Profile </button>
          <button className="menu-options"> Profile </button>

        </div>

        
    
      </div>

        </div>
        
        
        </motion.div>
    );
}

export default AdminDash;