import React from "react";
import {Navigate, Router, Routes, Route, useLocation} from 'react-router-dom'
import Login from './pages/login'
import AdminDash from './pages/AdminDash'


import { AnimatePresence } from 'framer-motion'
 

function AnimatedRoutes(){
     const location = useLocation();
    return (
      <AnimatePresence>
       <Routes location={location} key={location.pathname}>
        {/* the pages you can animate*/}
        <Route path='/welcome'  element={<WelcomePage/>}/>
        <Route path= '/login' element={<Login/>}/>
        <Route path= '/admin-dash' element={<AdminDash/>}/>

      </Routes>
      </AnimatePresence>
    );
}

export default AnimatedRoutes;