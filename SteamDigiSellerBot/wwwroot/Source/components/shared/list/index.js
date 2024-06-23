import React, { useEffect, useState } from "react";
import CircularProgress from "@mui/material/CircularProgress";
import css from "./styles.scss";

const list = ({ data, headers, itemRenderer, isLoading, loadingText }) => {
  let headerIdx = 0;

  return (
    <div className={css.wrapper}>
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
          <tbody>
            {data?.map((i) => {
              return itemRenderer(i);
            })}

            {/* {isLoading && (
              <tr>
                <td colSpan={headers.length}>
                  <div className={css.dump}>
                    <div className={css.loader}>
                      <CircularProgress
                        color="inherit"
                        sx={{
                          height: '99px !important',
                          width: '99px !important',
                        }}
                      />
                    </div>
                    <div>{loadingText}</div>
                  </div>
                </td>
              </tr>
            )} */}
          </tbody>
        </table>
      </div>

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
    </div>
  );
};

export default list;
