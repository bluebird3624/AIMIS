import { useState } from "react";
import { useNavigate } from "react-router-dom";
import * as icons from 'react-icons/io5';
import  {motion} from 'framer-motion'

function Adminfeedback(){
return(
     <motion.div
  initial={{ x: -100, opacity: 0 }}
  animate={{ x: 0, opacity: 1 }}
  transition={{ 
    duration: 1.7,
    ease: [0.25, 0.46, 0.45, 0.94] 
  }}
>
    <h1 style={{ fontFamily:"arial", fontSize: " 35px", marginLeft: "20px"}}> Feedback</h1>
    <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}> Feedback , suggestions and complaints from students</p>
    <div className="feed-card-section">


        {/** FEEDBACK CARD */}
        <div className="feed-card">
            <div className="feed-card-toprow">
                <div className="feed-title"> I like it here </div>

            </div>
            <div className="feed-card-secondrow">
                <div className="feed-student">
                    <icons.IoPersonOutline/>
                    Student name
                </div>
                <div className="feed-date">
                    <icons.IoTimeOutline/>
                    2025-9-13

                </div>

            </div>
            <div className="feed-message">
                <p>
                    Baby Shark, doo-doo, doo-doo, doo-doo
Baby Shark, doo-doo, doo-doo, doo-doo
Baby Shark, doo-doo, doo-doo, doo-doo
Baby Shark
Mommy Shark, doo-doo, doo-doo, doo-doo
Mommy Shark, doo-doo, doo-doo, doo-doo
Mommy Shark, doo-doo, doo-doo, doo-doo
Mommy Shark
Daddy Shark, doo-doo, doo-doo, doo-doo
Daddy Shark, doo-doo, doo-doo, doo-doo
Daddy Shark, doo-doo, doo-doo, doo-doo
Daddy Shark
Grandma Shark, doo-doo, doo-doo, doo-doo
Grandma Shark, doo-doo, doo-doo, doo-doo
Grandma Shark, doo-doo, doo-doo, doo-doo
Grandma Shark
Grandpa Shark, doo-doo, doo-doo, doo-doo
Grandpa Shark, doo-doo, doo-doo, doo-doo
Grandpa Shark, doo-doo, doo-doo, doo-doo
Grandpa Shark
                </p>

            </div>
            <div className="feed-card-thirdrow">
                <button className="reply-button"> Reply</button>

            </div>

        </div>
        {/* END OF FEEDBACK CARD */}

    </div>


    </motion.div>
);
}

export default Adminfeedback;