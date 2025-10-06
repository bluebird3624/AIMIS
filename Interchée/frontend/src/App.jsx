import WelcomePage from './pages/WelcomePage' 
import Onboarding from  './pages/Onboarding' 
import Login from './pages/login'
import AdminDash from './pages/AdminDash'
//useLocation is for framer-motion package
import {Navigate, Router, Routes, Route, BrowserRouter, useLocation} from 'react-router-dom'
import { AnimatePresence, motion } from 'framer-motion';

function AnimatedRoute({ children }) {
  const location = useLocation();
  return (
    <motion.div
      key={location.pathname}
      initial={{ x: '100%' }}
      animate={{ x: 0 }}
      exit={{ x: '-100%' }}
      transition={{ type: 'keyframes', duration: 1.2, ease: 'linear' }}
      className="absolute inset-0"
    >
      {children}
    </motion.div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AnimatePresence mode="sync">
        <Routes>
          <Route path='/welcome'  element={<WelcomePage/>}/>
          <Route path= '/login' element={<AnimatedRoute><Login/></AnimatedRoute>}/>
          <Route path= '/admin-dash' element={<AnimatedRoute><AdminDash/></AnimatedRoute>}/>
        </Routes>
      </AnimatePresence>
    </BrowserRouter>
  );
}

export default App;