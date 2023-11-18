import React from 'react';
import CircularProgress from '@mui/material/CircularProgress';

const CircularLoader = ({ height, width, color }) => {
  return (
    <div style={{ color: color || 'black' }}>
      <CircularProgress
        color={'inherit'}
        sx={{
          height: `${height || '99'}px !important`,
          width: `${width || '99'}px !important`,
        }}
      />
    </div>
  );
};

export default CircularLoader;
