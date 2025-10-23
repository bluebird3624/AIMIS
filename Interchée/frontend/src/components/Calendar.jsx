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
          className="calendar-main"
        />
      </Paper>
    </LocalizationProvider>
    </>
    );
}

export default Calendar;