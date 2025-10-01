
import WelcomePage from './pages/WelcomePage' 
import Onboarding from  './pages/Onboarding' 
import Login from './pages/login'
import AdminDash from './pages/AdminDash'
import {Navigate, Router, Routes, Route, BrowserRouter} from 'react-router-dom'


function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path='/welcome'  element={<WelcomePage/>}/>
        <Route path= '/login' element={<Login/>}/>
        <Route path= '/admin-dash' element={<AdminDash/>}/>

      </Routes>
    </BrowserRouter>
  );
}

export default App;

