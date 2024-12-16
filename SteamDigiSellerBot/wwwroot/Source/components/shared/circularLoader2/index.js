import React from "react";
import CircularProgress from "@mui/material/CircularProgress";
import css from "./styles.scss";
const CircularLoader = ({ height, width, color }) => {
  return (
    <div className={css.loaderContainer} style={{ color: color || "black" }}>
      <CircularProgress
        color={"inherit"}
        sx={{
          height: `${height || "99"}px !important`,
          width: `${width || "99"}px !important`,
        }}
      />
    </div>
  );
};

export default CircularLoader;
