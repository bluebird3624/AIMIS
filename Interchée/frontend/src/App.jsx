import WelcomePage from './pages/WelcomePage' 
import Onboarding from  './pages/Onboarding' 
import Login from './pages/login'
import AdminDash from './pages/AdminDash'
import ProtectedRoute from './components/ProtectedRoute'
import { AuthProvider } from './services/authContext'
import {Navigate, Router, Routes, Route, BrowserRouter, useLocation} from 'react-router-dom'
import { AnimatePresence, motion } from 'framer-motion';
import Calendar from './components/Calendar'
import { roles } from './utils/constants'


function AnimatedRoute({ children }) {
  const location = useLocation();
  return (
    <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 2, ease: "easeInOut" }}
        >
      {children}
    </motion.div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AnimatePresence mode="sync">
        <AuthProvider>
          <Routes>
            {/**public routes  */}
            <Route path='/welcome'  element={<WelcomePage/>}/>
            <Route path= '/login' element={<AnimatedRoute><Login/></AnimatedRoute>}/>
            <Route path='/Onboarding' element={<AnimatedRoute><Onboarding/></AnimatedRoute>}/>
            <Route path ='*' element={<WelcomePage/>}/>

            {/**protected routes general access */}
            <Route element={<ProtectedRoute/>}>
              {/* <Route path= '/profile' element={<Profile/>}/> */}
              {/* <Route path="/calendar" element={<Calendar/>}/> */}
              <Route path="/admin-dash" element={<AdminDash/>}/>
              <Route path='/Calendar' element={<AnimatedRoute><Calendar/></AnimatedRoute>}/>
            </Route>

            {/**protected admin routes */}
            <Route element={<ProtectedRoute allowedRoles={[roles.ADMIN]}/>}>

              
              {/* <Route path="/users" element={<Users/>}/>
              <Route path="/absence-admin" element={<AdminAbsence/>}/> */}
             
            
            </Route>
          </Routes>
        </AuthProvider>
      </AnimatePresence>
    </BrowserRouter>
  );
}

export default App;