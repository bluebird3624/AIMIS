import { useState, useEffect } from "react";
import '../Styles/dashboards.css'
import { useNavigate } from "react-router-dom";
import { motion } from 'framer-motion';
import  Group from '../assets/Group.svg';
import Sidebar from "../components/Layout/Sidebar";
import { IoSearch } from "react-icons/io5";



function AdminDash() {
    const navigate = useNavigate();
    const [isHovered, setIsHovered] = useState(false);
    const [isActive, setIsActive] = useState(false);

    const ProfileshowSolid = isHovered || isActive;
    const UsersshowSolid = isHovered || isActive;

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
            <div className="search-bar">
              < IoSearch className="search-icon"></IoSearch>
              <input 
              className="search-input"
              placeholder=" I'm looking for....."

              >

              </input>
            </div>
          </div>


          {/* LOGO */}
          <div className="topleft-container" >
            <div className="logo-container">
              <img
              src={Group}
              />
            </div>
             {/*  SIDEBAR */}
            <Sidebar/>

          </div>
            
          

          
       
         

        </div>
    
      </motion.div>
    );
}

export default AdminDash;