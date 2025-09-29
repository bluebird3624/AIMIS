
import WelcomePage from './pages/WelcomePage' 
import Onboarding from  './pages/Onboarding' 
import {Navigate, Router, Routes, Route, BrowserRouter} from 'react-router-dom'


function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path='/welcome'  element={<WelcomePage/>}/>
        <Route path= '/Onboarding' element={<Onboarding/>}/>
      </Routes>
    </BrowserRouter>
  );
}

export default App;

