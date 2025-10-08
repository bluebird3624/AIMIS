import { useState, useEffect } from "react";
import '../Styles/dashboards.css'
import { useNavigate } from "react-router-dom";
import { motion } from 'framer-motion';
import  Group from '../assets/Group.svg';
import { IoPersonOutline , IoPerson , IoPeopleOutline, IoPeople  } from "react-icons/io5";



function AdminDash() {

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


                <button 
                className="sidebar-options"
                onMouseEnter={() => setIsHovered(true)}
                onMouseLeave={() => setIsHovered(false)}
                onMouseDown={() => setIsActive(true)}
                onMouseUp={() => setIsActive(false)}
                
                > 
                {ProfileshowSolid ?  <IoPerson/> : < IoPersonOutline />}  Profile 
                
                </button>

                <button 
                className="sidebar-options"
                onMouseEnter={() => setIsHovered(true)}
                onMouseLeave={() => setIsHovered(false)}
                onMouseDown={() => setIsActive(true)}
                onMouseUp={() => setIsActive(false)}
                
                > 
                {UsersshowSolid ?  <IoPeople/> : < IoPeopleOutline />}  Users
                
                </button>

                



              </div>

            </div>

          </div>
          
       
         

        </div>
    
      </motion.div>
    );
}

export default AdminDash;