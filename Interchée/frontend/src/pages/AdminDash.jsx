
import '../Styles/dashboards.css'

import { motion } from 'framer-motion';
import  Group from '../assets/Group.svg';
import Sidebar from "../components/Layout/Sidebar";



function AdminDash() {
   
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
       
          

          </div>
          
       
        <Sidebar/>
      
         

        </div>
    
      </motion.div>
    );
}

export default AdminDash;