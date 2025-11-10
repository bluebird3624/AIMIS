import '../Styles/dashboards.css'
import { motion } from 'framer-motion';
import  * as icons from 'react-icons/io5';
import React, {useState, useEffect} from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {roles} from '../utils/constants';
import SearchBar from '../components/SearchBar';
import UserProfile from '../components/UserProfile';
import dashGroup from '../assets/dashGroup.svg';
import Homebutton from '../components/homepage';
import Userspage from '../components/Userspage';
import Adminabsence from '../components/AdminAbsence';
import Adminfeedback from '../components/Adminfeedback';
import ProfilePage from '../components/ProfilePage';
import Calendar from '../components/Calendar';
import { getCurrentUser } from '../services/auth';
import StudentAssignment from '../components/StudentAssignment';
import SupervisorReports from '../components/SupervisorReports';
import StudentAbsence from '../components/StudentAbsence';
import Adminreviews from '../components/AdminReviews';
import SupervisorReviews from '../components/SupervisorReviews';
import StudentReviews from '../components/StudentReviews';
import StudentFeedback from '../components/StudentFeedback';



const componentMap = {
  
  adminDashboard: Homebutton,
  users: Userspage,
  adminAbsence: Adminabsence,
  calendar: Calendar,
  feedback: Adminfeedback,
  profilePage: ProfilePage,
  studentAssignment: StudentAssignment,
  supervisorReports: SupervisorReports,
  studentAbsence: StudentAbsence,
  adminReviews: Adminreviews,
  supervisorReviews: SupervisorReviews,
  studentReviews: StudentReviews,
  studentFeedback: StudentFeedback,


  default: Homebutton
};

const sidebarConfig = {
  adminDashboard: {
    id: 'adminDashboard',
    name: 'Home',
    path: '/admin-dash',
    iconOutline: icons.IoHomeOutline,
    iconSolid: icons.IoHome,
    roles: [roles.ADMIN,roles.SUPERVISOR,roles.ATTACHEE, roles.INTERN]
  },

  users: {
    id: 'users',
    name: 'Users',
    path: '/users',
    iconOutline: icons.IoPeopleOutline,
    iconSolid: icons.IoPeople,
    roles: [roles.ADMIN, roles.HR]
  },



  adminAbsence: {
    id: 'adminAbsence',
    name: 'Absence',
    path: '/absence',
    iconOutline: icons.IoWalkOutline,
    iconSolid: icons.IoWalk,
    roles: [roles.ADMIN, roles.HR]
  },



  feedback: {
    id: 'feedback',
    name: 'Feedback',
    iconOutline: icons.IoChatbubblesOutline,
    iconSolid: icons.IoChatbubblesSharp,
    roles: [roles.ADMIN]
  },

  calendar: {
    id: 'calendar',
    name: 'Calendar',
    path: '/calendar',
    iconOutline: icons.IoCalendarOutline,
    iconSolid: icons.IoCalendar,
    roles: [roles.ADMIN, roles.ATTACHEE, roles.HR, roles.INTERN, roles.SUPERVISOR]

  },

   supervisorReports: {
    id: 'supervisorReports',
    name: 'Reports',
    path: '/supervisorReports',
    iconOutline: icons.IoBarChartOutline,
    iconSolid: icons.IoBarChart,
    roles: [roles.SUPERVISOR]

  },

  studentAssignment: {
    id: 'studentAssignment',
    name: 'Reports',
    iconOutline: icons.IoBarChartOutline,
    iconSolid: icons.IoBarChart,
    roles: [roles.ATTACHEE, roles.INTERN]

  },


  studentAbsence: {
    id: 'studentAbsence',
    name: 'Absence',
    iconOutline: icons.IoWalkOutline,
    iconSolid: icons.IoWalk,
    roles: [roles.ATTACHEE, roles.INTERN]

  },

  adminReviews: {
    id : 'adminReviews',
    name : 'Reviews',
    iconOutline : icons.IoBookOutline,
    iconSolid : icons.IoBook,
    roles : [roles.ADMIN]
  },

  supervisorReviews: {
    id : 'supervisorReviews',
    name : 'Reviews',
    iconOutline : icons.IoBookOutline,
    iconSolid : icons.IoBook,
    roles : [roles.SUPERVISOR]
  },

  studentReviews: {
    id : 'studentReviews', 
    name : 'Reviews',
    iconOutline : icons.IoBookOutline,
    iconSolid : icons.IoBook,
    roles : [roles.ATTACHEE, roles.INTERN]
  },

  studentFeedback:{
    id : 'studentFeedback',
    name : 'Feedback',
    iconOutline : icons.IoChatbubblesOutline,
    iconSolid : icons.IoChatbubblesSharp,
    roles : [roles.ATTACHEE, roles.INTERN]
  }

};

const getUserRole = () => {
  try {
    const userData = getCurrentUser();
   
    if (userData) {
      
      return userData.role || 'invalid'; 
    }
    return 'invalid';
  } catch (error) {
    console.error('Error getting user role:', error);
    return 'Employee';
  }
};



const getFilteredSidebarItems = (userRole) => {
  const filteredItems = Object.values(sidebarConfig).filter(item =>
    item.roles.includes(userRole)
  );
 
 return filteredItems; 
};

const SidebarItem = ({ item, isActive, onClick }) => {
  const [isHovered, setIsHovered] = useState(false);
  const IconOutline = item.iconOutline;
  const IconSolid = item.iconSolid;

  return (
    <button 
      className={`sidebar-options ${isActive ? 'active' : ''} ${isHovered ? 'hover' : ''}`}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onClick={() => onClick(item)}
    > 
      {isHovered || isActive? <IconSolid /> : <IconOutline />}
      <span>{item.name}</span>
    </button>
   
  );
};

const MainContentRenderer = ({ activeItemId }) => {
  console.log('active item id: ', activeItemId);
  const ComponentToRender = componentMap[activeItemId] ;
  
  console.log('compojnent to render: ', componentMap[activeItemId] );
  return (
    <div className="main-content">
      <ComponentToRender />
    </div>
  );
};


function AdminDash() {
  const [activeItem, setActiveItem] = useState('adminDashboard');
  const [sidebarItems, setSidebarItems] = useState([]);
  const [userRole, setUserRole] = useState('');
  const navigate = useNavigate();
  const location = useLocation();

 
  useEffect(() => {
    const userRole = getUserRole();
    setUserRole(userRole);
    const filteredItems = getFilteredSidebarItems(userRole);
    setSidebarItems(filteredItems);
  }, []);

 
  

  const handleItemClick = (item) => {
    console.log('item clicked: ', item);
    setActiveItem(item.id);
  };

  return (
     <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      transition={{
        type: "tween",
        ease: "anticipate",
        duration: 2.1
      }}
    >
    <div className="dashboard-layout">
    
      <div className="dashboard-header">
   
         <div className="header-logo">
          <img src={dashGroup} alt="Company Logo" className="logo-image" />
        </div>

        <div className="header-search">
          <SearchBar sidebarItems={sidebarItems} userRole={userRole} />
        </div>

   
        <div className="header-profile">
          <UserProfile setActiveItem={setActiveItem}/>
          
        </div>
      </div>

  
      <div className="dashboard-main">

         
        <div className="dashboard-sidebar">
         
          <div className="options-container">
            {sidebarItems.map((item) => (
              <SidebarItem
                key={item.id}
                item={item}
                isActive={activeItem === item.id}
                onClick={handleItemClick}
              />
            ))}
          </div>
        </div>

        <MainContentRenderer activeItemId={activeItem}/>

       
      </div>
    </div>
   </motion.div>
  );
  
}

export default AdminDash;


 