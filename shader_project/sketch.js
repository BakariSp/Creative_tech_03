let windowHeight = window.innerHeight;
let windowWidth = window.innerWidth;

let myShader;

function preload() {
  // load each shader file (don't worry, we will come back to these!)
  myShader = loadShader('shader.vert', 'shader.frag');
}

function setup() {
  // the canvas has to be created with WEBGL mode
  createCanvas(windowWidth, windowHeight, WEBGL);
  describe('a simple shader example that outputs the color red')
}

function draw() {
  noCursor();
  // shader() sets the active shader, which will be applied to what is drawn next
  shader(myShader);
  myShader.setUniform('u_resolution', [width, height]);
  myShader.setUniform('u_mouse', [mouseX / windowWidth, 1 - mouseY / windowHeight]);
  myShader.setUniform("u_time", millis() / 1000.0);
  console.log(millis() / 1000.0);
  // apply the shader to a rectangle taking up the full canvas
  rect(0,0,width,height);
}