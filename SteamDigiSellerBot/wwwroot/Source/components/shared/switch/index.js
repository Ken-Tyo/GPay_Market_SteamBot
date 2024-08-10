import * as React from 'react';
import css from './styles.scss';

const SwitchBtn = ({ value, onChange, style, lastSaveTime }) => {

  const [checked, setChecked] = React.useState(false);
  const [componentKey, setComponentKey] = React.useState(Date.now());

  const hasMoreThanOneMinutePassed = (lastSaveTime) => {
    if (!lastSaveTime) return true;
    try {
        const lastSaveDate = new Date(lastSaveTime + 'Z'); // Ensure lastSaveTime is in UTC
        const currentTime = new Date();
        const timeDiff = currentTime.getTime() - lastSaveDate.getTime();
        const diffMin = timeDiff / 60000; // Convert milliseconds to minutes
        return diffMin > 1;
    } catch (error) {
        console.error('Error processing lastSaveTime:', error);
        return false;
    }
    };
  const timeDiffSeconds = (lastSaveTime) => {
        try {
            const lastSaveDate = new Date(lastSaveTime + 'Z'); // Ensure lastSaveTime is in UTC
            const currentTime = new Date();
            const timeDiff = currentTime.getTime() - lastSaveDate.getTime();
            const diffSec = timeDiff / 1000; // Convert milliseconds to minutes
            return '0:' + diffSec.toString();
        } catch (error) {
            console.error('Error processing lastSaveTime:', error);
            return '';
        }
    };

    React.useEffect(() => {
    setChecked(value && hasMoreThanOneMinutePassed(lastSaveTime));
  }, [value, lastSaveTime]);

  React.useEffect(() => {
    const checkInterval = setInterval(() => {
        if (!lastSaveTime) return; 

        try {
            if (hasMoreThanOneMinutePassed(lastSaveTime)) {
                setChecked(value);
            }
        } catch (error) {
            console.error('Error processing lastSaveTime:', error);
        }
    }, 15000);

    return () => clearInterval(checkInterval);
  }, [lastSaveTime]);

  return (
      <div className={css.wrapper} style={{ ...style }} key={componentKey}>
          {hasMoreThanOneMinutePassed(lastSaveTime) &&
              (<div
                  className={css.track + ' ' + (checked ? css.checked : '')}
                  onClick={() => {
                      if (hasMoreThanOneMinutePassed(lastSaveTime)) {
                          if (onChange) onChange(!checked);
                          setChecked(!checked);
                      }
                  }}>
                  <div className={css.thumb + ' ' + (checked ? css.checked : '')}></div>
              </div>)}
          {!hasMoreThanOneMinutePassed(lastSaveTime) &&
              (<div className={css.track}>{timeDiffSeconds(lastSaveTime)}</div>)}
    </div>
  );
};

export default SwitchBtn;
