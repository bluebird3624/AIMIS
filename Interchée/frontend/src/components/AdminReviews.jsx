import React, {useState, useEffect} from 'react';
import {motion} from 'framer-motion'

function Adminreviews (){

    return(
     <motion.div
  initial={{ x: -100, opacity: 0 }}
  animate={{ x: 0, opacity: 1 }}
  transition={{ 
    duration: 1.7,
    ease: [0.25, 0.46, 0.45, 0.94] 
  }}
>
    <h1 style={{ fontFamily:"arial", fontSize: " 35px", marginLeft: "20px"}}> Reviews</h1>
    <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}> Review information between supervisors and students</p>
    </motion.div>

    );

}

export default Adminreviews;

