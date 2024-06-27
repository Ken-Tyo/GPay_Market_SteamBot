import React, { useEffect, useRef, useState } from "react";
import CircularProgress from "@mui/material/CircularProgress";
import css from "./styles.scss";
import { ViewportList } from "react-viewport-list";

const list = ({
  data,
  headers,
  itemRenderer,
  isLoading,
  loadingText,
  componentDidMount = null,
}) => {
  let headerIdx = 0;
  const ref = useRef(null);
  return (
    <div className={css.wrapper}>
      {isLoading && (
        <div className={css.dump}>
          <div className={css.loader}>
            <CircularProgress
              color="inherit"
              sx={{
                height: "99px !important",
                width: "99px !important",
              }}
            />
          </div>
          <div>
            {
              //Так сделано, чтобы была возможность при изменении внешних значений мог рассчитываться текст
              loadingText()
            }
          </div>
        </div>
      )}
      <div style={{ overflow: isLoading ? "hidden" : "inherit" }}>
        <table>
          <thead>
            <tr class="head">
              {headers?.map((h) => {
                let positionCss = "";
                if (headerIdx === 0) positionCss = css.first;
                else if (headerIdx === headers.length - 1)
                  positionCss = css.last;

                headerIdx++;
                return (
                  <th>
                    <div className={positionCss}>{h}</div>
                  </th>
                );
              })}
            </tr>
          </thead>

          <tbody className="scroll-container" ref={ref}>
            {
              <ViewportList
                viewportRef={ref}
                items={data}
                itemMinSize={40}
                margin={8}
                overscan={15}
              >
                {(i) => itemRenderer(i)}
              </ViewportList>
            }
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default list;
