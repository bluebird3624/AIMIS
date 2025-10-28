import React, {useState, useEffect} from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import '../Styles/calendar.css';
import { Paper, Typography, Box } from '@mui/material';
import { DateCalendar } from '@mui/x-date-pickers/DateCalendar';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { format } from 'date-fns';

function Calendar() {

    const [selectedDate, setSelectedDate] = useState(new Date());


    return(
        <>
        <h1 className='heading'> Calendar</h1>
         <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Paper elevation={3} className="calendar-container">
        {/* Current Month and Year Header */}
        <Box className="calendar-header">
          <Typography variant="h4" className="month-year-text">
            {format(selectedDate, 'MMMM yyyy')}
          </Typography>
          <Typography variant="subtitle1" className="full-date-text">
            {format(selectedDate, 'EEEE, MMMM d')}
          </Typography>
        </Box>

        {/* Calendar Component */}
      <DateCalendar 
      style = {{minheight: '800px'}}
  sx={{
    // Weekday header row
    '& .MuiDayCalendar-header': {
      display: 'flex',
      justifyContent: 'space-between',
      padding: '10px 0',
      marginBottom: '8px',
      borderBottom: '1px solid #e0e0e0',
      fontSize: '40px'
    },
    
    // Individual weekday cells
    '& .MuiDayCalendar-weekDayLabel': {
      width: '30px',
      height: '30px',
      fontSize: '30px',
      fontWeight: 700,
      color: '#747474ff', // Change color
      backgroundColor: '#f5f5f5', // Add background
      borderRadius: '4px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      margin: '1px',
    },
    
    // INDIVIDUAL DATES - LARGER SIZE
    '& .MuiPickersDay-root': {
      width: '40px',      // Match weekday width
      height: '40px',     // Match weekday height
      fontSize: '20px',   // Large font size for dates
      fontWeight: 400,
      borderRadius: '4px',
      margin: '20px',
    },
    
    // Selected date styling
    '& .MuiPickersDay-root.Mui-selected': {
      backgroundColor: '#1976d2',
      color: 'white',
      fontSize: '26px',   // Slightly larger when selected
      fontWeight: 'bold',
    },
    
    // Today's date
    '& .MuiPickersDay-today': {
      border: '2px solid #1976d2',
      backgroundColor: 'transparent',
    },
    
    // Hover effects
    '& .MuiPickersDay-root:hover': {
      backgroundColor: 'rgba(25, 118, 210, 0.43)',
      transform: 'scale(1.05)',
    },
    
    // Ensure calendar container can fit the larger dates
    '& .MuiDayCalendar-monthContainer': {
      width: '100%',
    },
    
    // Adjust the overall calendar size if needed
    width: 500,
    height: 450,
  }}
/>
      </Paper>
    </LocalizationProvider>
    </>
    );
}

export default Calendar;