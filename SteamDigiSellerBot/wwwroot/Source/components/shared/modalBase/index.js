import * as React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import CircularProgress from '@mui/material/CircularProgress';

export default function ModalBase({
  isOpen,
  children,
  title,
  height,
  width,
  isLoading,
}) {
  return (
    <div>
      <Dialog
        open={isOpen}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
        sx={{
          '& .MuiPaper-root': {
            borderRadius: '34px',
            backgroundColor: '#7935A3',
            height: height || 212,
            width: width || 705,
            maxWidth: width || 705,
            overflowY: 'hidden',

            '&::-webkit-scrollbar': {
              width: '0px',
            },
            '&::-webkit-scrollbar-track': {
              backgroundColor: 'transparent',
            },
            '&::-webkit-scrollbar-thumb': {
              //background-color: #7D5292;
              backgroundColor: 'transparent',
              marginTop: '30px',
              //box-shadow: inset 0 0 6px rgba(0, 0, 0, 0.3);
            },
          },
        }}
      >
        <div
          style={{
            display: 'flex',
            flexDirection: 'column',
            //flexFlow: 'column',
            height: '100%',
          }}
        >
          {title && (
            <div style={{ flex: '0 1 auto' }}>
              <DialogTitle
                sx={{
                  color: '#FFFFFF',
                  fontSize: '28px',
                  lineHeight: '28px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  marginTop: '17px',
                }}
                id="alert-dialog-title"
              >
                {title}
              </DialogTitle>
            </div>
          )}
          {!isLoading && children}
          {isLoading && (
            <div
              style={{
                color: 'white',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: '619px',
              }}
            >
              <CircularProgress
                color="inherit"
                sx={{
                  height: '99px !important',
                  width: '99px !important',
                }}
              />
            </div>
          )}
        </div>
      </Dialog>
    </div>
  );
}
