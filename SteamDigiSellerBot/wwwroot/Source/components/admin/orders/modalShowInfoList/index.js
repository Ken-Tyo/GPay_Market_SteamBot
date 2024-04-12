import React, { useState } from "react";
import Button from "../../../shared/button";
import ModalBase from "../../../shared/modalBase";
import css from "./styles.scss";
import { getUniqueCodeHref } from "../../../../utils/common";
import { state } from "../../../../containers/admin/state";
const modalShowInfoList = ({
  title,
  placeholder,
  isOpen,
  onOk,
  width,
  height,
}) => {

  const {
    newUniqueCodes,
  } = state.use();
  return (
    <ModalBase
      isOpen={isOpen}
      title={title}
      width={width || 705}
      height={height || 527}
    >
       <div className={css.content}>
        <div className={css.boxes}>
          <div>
            <textarea readonly
              rows="18"
              class={css.textarea}
              defaultValue={newUniqueCodes.map(e => getUniqueCodeHref(e)).join("\n")}
            ></textarea>
          </div>
        </div>
      </div>

      <div className={css.actions}>
        <Button
          text={onOk.label}
          style={{
            backgroundColor: "#478C35",
            marginRight: "36px",
            width: "284px",
          }}
          onClick={() => {
            if (onOk.action) onOk.action();
          }}
        />
      </div>
    </ModalBase>
  );
};

export default modalShowInfoList;
