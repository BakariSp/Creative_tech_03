#ifdef GL_ES
precision mediump float;
#endif

uniform vec2 u_resolution; // This is passed in as a uniform from the sketch.js file
uniform vec2 u_mouse;
uniform float u_time;

vec2 fade(vec2 t){return sin(u_time)*t*t*t*(t*(t*6.0-15.0)+10.0);}
vec4 permute(vec4 x){return mod(((x*34.0)+1.0)*x, 289.0);}

float cnoise(vec2 P){
  vec4 Pi = floor(P.xyxy) + vec4(0.0, 0.0, 1.0, 1.0);
  vec4 Pf = fract(P.xyxy) - vec4(0.0, 0.0, 1.0, 1.0);
  Pi = mod(Pi, 289.0); // To avoid truncation effects in permutation
  vec4 ix = Pi.xzxz;
  vec4 iy = Pi.yyww;
  vec4 fx = Pf.xzxz;
  vec4 fy = Pf.yyww;
  vec4 i = permute(permute(ix) + iy);
  vec4 gx = 2.0 * fract(i * 0.0243902439) - 1.0; // 1/41 = 0.024...
  vec4 gy = abs(gx) - 0.5;
  vec4 tx = floor(gx + 0.5);
  gx = gx - tx;
  vec2 g00 = vec2(gx.x,gy.x);
  vec2 g10 = vec2(gx.y,gy.y);
  vec2 g01 = vec2(gx.z,gy.z);
  vec2 g11 = vec2(gx.w,gy.w);
  vec4 norm = 1.79284291400159 - 0.85373472095314 * 
    vec4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11));
  g00 *= norm.x;
  g01 *= norm.y;
  g10 *= norm.z;
  g11 *= norm.w;
  float n00 = dot(g00, vec2(fx.x, fy.x));
  float n10 = dot(g10, vec2(fx.y, fy.y));
  float n01 = dot(g01, vec2(fx.z, fy.z));
  float n11 = dot(g11, vec2(fx.w, fy.w));
  vec2 fade_xy = fade(Pf.xy);
  vec2 n_x = mix(vec2(n00, n01), vec2(n10, n11), fade_xy.x);
  float n_xy = mix(n_x.x, n_x.y, fade_xy.y);
  return 2.3 * n_xy;
}

void main() {

  // position of the pixel divided by resolution, to get normalized positions on the canvas
  vec2 st = gl_FragCoord.xy/u_resolution.xy;
  vec2 mouse = u_mouse;
  // st.x = st.x * 2.0 - 1.0;
  mouse.x *= u_resolution.x / u_resolution.y;
  st.x = st.x * u_resolution.x / u_resolution.y;
  //st.x = st.x * 0.5 + 1.0;

  float shade = cnoise(st * 5.0);
  shade = sin(shade * 20.0);
  shade = step(0.5, shade);
  vec3 blackCol = vec3(1.0, 0.0, 0.0);
  vec3 uvColor = vec3(st, 1.0);
  vec3 mixedColor = mix(blackCol, uvColor, shade);
  
  vec3 value = vec3(1.0 - distance(st, mouse) * 5.0, 0.0, 0.5);
  vec3 center = 1.0 - vec3(distance(st, mouse) * 2.0) ;
  vec4 col = vec4(mixedColor * value + (1.0 - center) * 0.5 * sin(u_time), 1.0);
  gl_FragColor = col;

  // Lets use the pixels position on the x-axis as our gradient for the red color
  // Where the position is closer to 0.0 we get black (st.x = 0.0)
  // Where the position is closer to width (defined as 1.0) we get red (st.x = 1.0)
  
  // gl_FragColor = col * value; // R,G,B,A

  // you can only have one gl_FragColor active at a time, but try commenting the others out
  // try the green component

  //gl_FragColor = vec4(0.0,st.x,0.0,1.0); 

  // try both the x position and the y position
  
  //gl_FragColor = vec4(st.x,st.y,0.0,1.0); 
}