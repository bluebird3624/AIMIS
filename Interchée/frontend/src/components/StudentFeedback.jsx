import { useState } from "react";
import '../styles/feedback.css';
import * as icons from 'react-icons/io5';
import {motion} from 'framer-motion'


export default function StudentFeedback() { 

    const [showForm, setShowForm] = useState(false);

    const handleGiveFeedback = () => {
        setShowForm(true);
    }

    return (
         <motion.div
  initial={{ x: -100, opacity: 0 }}
  animate={{ x: 0, opacity: 1 }}
  transition={{ 
    duration: 1.7,
    ease: [0.25, 0.46, 0.45, 0.94] 
  }}
>
         <h1 style={{ fontFamily:"arial", fontSize: " 35px", marginLeft: "20px"}}> Feedback</h1>
         <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}> Hand in feedback, suggestions or complaints</p>
         <button 
         className="feedback-button"
         onClick={handleGiveFeedback} >
            <icons.IoChatbubbleEllipsesOutline/> 
            Give Feedback
         </button>


         {/* Feedback Form Modal */}
            {showForm && (
               <div className="feed-modal-overlay">
               <div className="feed-modal-container">
                                       <div className="feed-modal-header">
                                           <h2 className="feed-modal-title"> New Feedback</h2>
                                           <button 
                                               className="feed-modal-close"
                                               onClick={() => setShowForm(false)}
                                           >
                                               <icons.IoClose />
                                           </button>
                                       </div>
                                       
                                       <div className="feed-modal-content">
                                           <div className="feed-form-group">
                                               <label className="feed-form-label">Feedback Title </label>
                                               <input
                                                   type="text"
                                                   className="feed-form-input"
                                                   placeholder="Enter review title"
                                                  
                                               />
                                           </div>
                                           <div className="feed-form-group">
                                               <label className="feed-form-label">Feedback Description</label>
                                               <textarea
                                                   className="feed-form-textarea"
                                                   placeholder="Enter review description"
                                                   rows="4"
                                
                                               />
                                           </div>
                                       </div>
               
                                       <div className="feed-modal-actions">
                                           <button 
                                               className="feed-cancel-btn"
                                           >
                                               Cancel
                                           </button>
                                           <button 
                                               className="feed-submit-btn"
                                           >
                                               Send feedback
                                           </button>
                                       </div>
                                   </div>
               </div> 
            )}
        </motion.div>
    );
}