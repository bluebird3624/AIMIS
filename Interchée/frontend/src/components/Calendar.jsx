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
      value={selectedDate}
      onChange={(newDate) => setSelectedDate(newDate)}
      sx={{
    // Calendar container
    width: '100%',
    height: 'auto',
    minHeight: '500px',
    
    // Weekday header row - FIXED ALIGNMENT
    '& .MuiDayCalendar-header': {
      display: 'grid',
      gridTemplateColumns: 'repeat(7, 1fr)',
      justifyContent: 'space-between',
      padding: '8px 0',
      marginBottom: '4px',
      borderBottom: '1px solid #e0e0e0',
      gap: '0px',
    },
    
    // Individual weekday cells - PERFECT ALIGNMENT
    '& .MuiDayCalendar-weekDayLabel': {
      width: '100%',
      height: '40px',
      fontSize: '16px',
      fontWeight: 700,
      color: '#747474',
      backgroundColor: '#f5f5f5',
      borderRadius: '4px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      margin: '0',
      boxSizing: 'border-box',
    },
    
    // Month grid container
    '& .MuiDayCalendar-monthContainer': {
      width: '100%',
      height: 'auto',
    },
    
    // Week rows
    '& .MuiDayCalendar-weekContainer': {
      display: 'grid',
      gridTemplateColumns: 'repeat(7, 1fr)',
      justifyContent: 'space-between',
      margin: '0',
      gap: '0px',
    },
    
    // INDIVIDUAL DATES - PERFECT ALIGNMENT
    '& .MuiPickersDay-root': {
      width: '100%',
      height: '50px',
      fontSize: '16px',
      fontWeight: 400,
      borderRadius: '4px',
      margin: '2px',
      minWidth: 'auto',
      boxSizing: 'border-box',
    },
    
    // Selected date styling
    '& .MuiPickersDay-root.Mui-selected': {
      backgroundColor: '#1976d2',
      color: 'white',
      fontSize: '16px',
      fontWeight: 'bold',
    },
    
    // Today's date
    '& .MuiPickersDay-today': {
      border: '2px solid #1976d2',
      backgroundColor: 'transparent',
    },
    
    // Hover effects
    '& .MuiPickersDay-root:hover': {
      backgroundColor: 'rgba(25, 118, 210, 0.1)',
      transform: 'scale(1.02)',
    },
    
    // Remove any default margins/padding that might cause misalignment
    '& .MuiPickersCalendarHeader-root': {
      marginBottom: '16px',
    },
    
    '& .MuiDayCalendar-slideTransition': {
      minHeight: '400px',
      height: 'auto',
    }
  }}
/>
      </Paper>
    </LocalizationProvider>
    </>
    );
}

export default Calendar;