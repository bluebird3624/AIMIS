import { useState, useEffect } from "react";
import '../Styles/dashboards.css'
import { useNavigate } from "react-router-dom";
import { motion } from 'framer-motion';
import  Group from '../assets/Group.svg';
import { IoPersonOutline , IoPerson  } from "react-icons/io5";


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

        <div className="dash-body">


          {/* STATUS BAR */}
          <div className="status-bar">

          </div>


          {/* LOGO */}
          <div className="topleft-container" >
            <div className="logo-container">
              <img
              src={Group}
              />
            </div>


            {/*  SIDEBAR */}
            <div className="sidebar">
              <div className="options-container">
                <button className="sidebar-options"> Profile </button>


              </div>

            </div>

          </div>
          
       
         

        </div>
    
      </motion.div>
    );
}

export default AdminDash;