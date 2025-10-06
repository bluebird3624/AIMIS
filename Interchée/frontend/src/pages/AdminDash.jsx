import { useState, useEffect } from "react";
import '../Styles/dashboards.css'
import { useNavigate } from "react-router-dom";
import { motion } from 'framer-motion';

function AdminDash() {
   
    return(
        <>
            <motion.div
                initial={{ x: '100%' }}
                animate={{ x: 0 }}
                exit={{ x: '-100%' }}
                transition={{ type : 'tween' , duration: 1.2  }}
                className="absolute inset-0"
            >
                <div className="dash-body" style={{ backgroundImage: "url('/src/assets/dashboards.png')"}} >
                </div>
            </motion.div>
        </>
    );
}

export default AdminDash;