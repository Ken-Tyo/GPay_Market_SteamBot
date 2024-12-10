import * as React from 'react';
import Colors from './colors'
import css from './styles.scss';

const SwitchBtn = ({ value, onChange, style, lastSaveTime, color }) => {

    const [checked, setChecked] = React.useState(value);
    const [timer, setTimer] = React.useState();

    const timeDiffSeconds = (lastSaveTime) => {
        if (!lastSaveTime) return 86400;
        try {
            const lastSaveDate = new Date(lastSaveTime + 'Z'); // Ensure lastSaveTime is in UTC
            const currentTime = new Date();
            const timeDiff = currentTime.getTime() - lastSaveDate.getTime();
            const diffSec = timeDiff / 1000; // Convert milliseconds to minutes
            return Math.floor(diffSec);
        } catch (error) {
            console.error('Error processing lastSaveTime:', error);
            return 86400;
        }
    };

    React.useEffect(() => {
        setChecked(value);
    }, [value]);

    React.useEffect(() => {
        const updateStates = () => {
            const seconds = timeDiffSeconds(lastSaveTime);
            setTimer(60 - seconds);
        };

        updateStates(); // Initial call to set the states immediately

        const intervalId = setInterval(updateStates, 1000); // Update states every 15 seconds

        return () => clearInterval(intervalId); // Cleanup interval on component unmount
    }, [lastSaveTime]); // Depend on lastSaveTime and value

    const getColorClass = (color) => {
      if (color === Colors.ORANGE) {
        return css.orange;
      }

      return css.green;
    }


  return (
      <div className={css.wrapper} style={{ ...style }}>
          {timer >= 0 ? (
              <div className={css.awaition}>{timer > 9 ? `0:${timer}` : `0:0${timer}`}</div>
          ) : (
              <div
                  className={css.track + ' ' + (checked ? css.checked : '')+ ' ' + getColorClass(color)}
                  onClick={() => {
                    if (onChange) onChange(!checked);
                    setChecked(!checked);}}>
                <div className={css.thumb + ' ' + (checked ? css.checked : '') + ' ' + getColorClass(color)}></div>
              </div>
          )}
    </div>
  );
};

export default SwitchBtn;
