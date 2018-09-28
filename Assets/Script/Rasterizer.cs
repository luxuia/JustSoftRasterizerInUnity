using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rasterizer {

    public Texture2D texture;

	public Rasterizer(Texture2D texture) {
        this.texture = texture;
    }

    void DrawPixel(int x, int y, ref Color color) {
        texture.SetPixel(x, y, color);
    }

    public void DrawLine(ref Vector2 start, ref Vector2 end, ref Color color) {
        //裁剪不可见的线
        if (!CohenSutherlandLineClip(ref start, ref end, new Vector2(100, 100), new Vector2(texture.width-100, texture.height-100))) {
            return;
        }

        int x0 = (int)start.x;
        int x1 = (int)end.x;

        int y0 = (int)start.y;
        int y1 = (int)end.y;
        
        if (x0 == x1 && y0 == y1) {
            //点
            DrawPixel(x0, y0, ref color);
        } else if (x0 == x1) {
            // 竖线
            int inc = y0 > y1 ? -1 : 1;
            for (var y = y0; y != y1; y += inc) DrawPixel(x0, y, ref color);
            DrawPixel(x1, y1, ref color);
        } else if (y0 == y1) {
            // 横线
            int inc = x0 > x1 ? -1 : 1;
            for (var x = x0; x != x1; x += inc) DrawPixel(x, y0, ref color);
            DrawPixel(x1, y1, ref color);
        } else {
            int dx = (x0 < x1) ? x1 - x0 : x0 - x1;
            int dy = (y0 < y1) ? y1 - y0 : y0 - y1;

            // k < 0
            if (dx > dy) {
                if (x1 < x0) {
                    Const.Swap(ref x0, ref x1);
                    Const.Swap(ref y0, ref y1);
                }

                int rem = 0, x, y;
                for (x = x0, y = y0; x<=x1; x++) {
                    DrawPixel(x, y, ref color);
                    rem += dy;
                    if (rem >= dx) {
                        rem -= dx;
                        y += (y1 > y0) ? 1 : -1;
                    }
                }
            } else {
                if (y1 < y0) {
                    Const.Swap(ref x0, ref x1);
                    Const.Swap(ref y0, ref y1);
                }
                int rem = 0, x, y;
                for (x = x0, y = y0; y<=y1; y++) {
                    DrawPixel(x, y, ref color);
                    rem += dx;
                    if (rem >= dy) {
                        rem -= dy;
                        x += (x1 > x0) ? 1 : -1;
                    }
                }
            }
        }
    }

    bool CohenSutherlandLineClip(ref Vector2 start, ref Vector2 end, Vector2 min, Vector2 max) {
        float x0 = start.x;
        float x1 = end.x;
        
        float y0 = start.y;
        float y1 = end.y;

        int encode0 = Encode(ref start, ref min, ref max);
        int encode1 = Encode(ref end, ref min, ref max);

        bool accept = false;

        while (true) {
            if ((encode0 | encode1) == 0) {
                accept = true;
                break;
            } else if ((encode0 & encode1) > 0) {
                // 起点终点都在边上
                break;
            } else {
                float x = 0, y = 0;
                int encodeOut = encode0 > 0 ? encode0 : encode1;

                float k = (y1 - y0) / (x1 - x0);

                // y = y0 + k(x-x0)
                // x = x0 + 1/k*(y-y0)
                if ((encodeOut & Const.ENCODE_SIDE_TOP)>0) {
                    y = max.y;
                    x = x0 + (y - y0) / k;
                } else if ((encodeOut & Const.ENCODE_SIDE_BOTTOM) > 0) {
                    y = min.y;
                    x = x0 + (y - y0) / k;
                } else if ((encodeOut & Const.ENCODE_SIDE_LEFT) > 0) {
                    x = min.x;
                    y = y0 + (x - x0) * k;
                } else if ((encodeOut&Const.ENCODE_SIDE_RIGHT) > 0) {
                    x = max.x;
                    y = y0 + (x - x0) * k;
                }

                if (encodeOut == encode0) {
                    x0 = x;
                    y0 = y;
                    var new_pos = new Vector2(x, y);
                    encode0 = Encode(ref new_pos, ref min, ref max);
                } else {
                    x1 = x;
                    y1 = y;
                    var new_pos = new Vector2(x, y);
                    encode1 = Encode(ref new_pos, ref min, ref max);
                }
            }
        }
        if (accept) {
            start.x = x0;
            start.y = y0;

            end.x = x1;
            end.y = y1;
        }
        return accept;
    }

    int Encode(ref Vector2 pos, ref Vector2 min, ref Vector2 max) {
        int code = Const.ENCODE_SIDE_INSIDE;

        if (pos.x < min.x)
            code |= Const.ENCODE_SIDE_LEFT;
        else if (pos.x > max.x)
            code |= Const.ENCODE_SIDE_RIGHT;

        if (pos.y < min.y)
            code |= Const.ENCODE_SIDE_BOTTOM;
        else if (pos.y > max.y)
            code |= Const.ENCODE_SIDE_TOP;

        return code;
    }
}
