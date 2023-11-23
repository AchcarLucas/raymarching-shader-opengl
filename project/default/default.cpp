#include "init.hpp"
#include "image.hpp"
#include "texture.hpp"
#include "object.hpp"
#include "shader.hpp"
#include "camera.hpp"
#include "mesh.hpp"
#include "ubo.hpp"

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

#define EX_MARCHING 04

static void processInput(GLFWwindow *, float);
static void mouseCallback(GLFWwindow*, double, double);
static void scrollCallback(GLFWwindow*, double, double);

// camera class
static Camera *camera = new Camera();

// timing
static float delta_time = 0.0f;	// time between current frame and last frame
static float last_frame = 0.0f;

static std::vector<Vertex> quad_vertices = {
    Vertex(glm::vec3(1.0f,  1.0f, 0.0f), glm::vec3(0.0f, 0.0f, -1.0f), glm::vec2(1.0f, 1.0f)),
    Vertex(glm::vec3(1.0f, -1.0f, 0.0f), glm::vec3(0.0f, 0.0f, -1.0f), glm::vec2(1.0f, 0.0f)),
    Vertex(glm::vec3(-1.0f, -1.0f, 0.0f), glm::vec3(0.0f, 0.0f, -1.0f), glm::vec2(0.0f, 0.0f)),
    Vertex(glm::vec3(-1.0f,  1.0f, 0.0f), glm::vec3(0.0f, 0.0f, -1.0f), glm::vec2(0.0f, 1.0f))
};

static std::vector<GLuint> quad_indices = {
    0, 1, 2,
    0, 2, 3

};

static void updateShader(const int width, const int height);

static UBO *ubo;

int run_default(const int width, const int height)
{
    _stbi_set_flip_vertically_on_load(true);

    #if EX_MARCHING == 00
        Shader *shader = new Shader("glsl/default/shader.vs", "glsl/default/shader.fs");
    #elif EX_MARCHING == 01
        Shader *shader = new Shader("glsl/ex_01/shader.vs", "glsl/ex_01/shader.fs");
    #elif EX_MARCHING == 02
        Shader *shader = new Shader("glsl/ex_02/shader.vs", "glsl/ex_02/shader.fs");
    #elif EX_MARCHING == 03
        Shader *shader = new Shader("glsl/christmas/shader.vs", "glsl/christmas/shader.fs");
    #elif EX_MARCHING == 04
        Shader *shader = new Shader("glsl/structures/shader.vs", "glsl/structures/shader.fs");
    #endif // EX_MARCHING

    ubo = new UBO("General", 2 * sizeof(glm::vec2) + sizeof(float), 0);

    glm::vec2 resolution = glm::vec2(width, height);
    ubo->UBOSubBuffer(glm::value_ptr(resolution), 0, sizeof(glm::vec2));

    shader->use();
    shader->setUniformBlockBinding(ubo->getName(), ubo->getBinding());

    std::vector<Texture2D*> textures = {};

    Mesh *quad = new Mesh(quad_vertices, quad_indices, textures, VERTEX_TYPE::ATTRIB_PNT);

    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
    glfwSetCursorPosCallback(window, mouseCallback);
    glfwSetScrollCallback(window, scrollCallback);

    glEnable(GL_DEPTH_TEST);

    while(!glfwWindowShouldClose(window)) {
        float current_frame = static_cast<float>(glfwGetTime());
        delta_time = current_frame - last_frame;
        last_frame = current_frame;

        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        glClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        updateShader(width, height);

        quad->draw(shader);

        processInput(window, delta_time);

        glfwPollEvents();
        glfwSwapBuffers(window);
    }

    delete shader;
    delete quad;
    delete ubo;

    glfwTerminate();
    return 0;
}

static void processInput(GLFWwindow *window, float delta_time)
{
    camera->processInput(window, delta_time);
}

static void mouseCallback(GLFWwindow* window, double x_pos, double y_pos)
{
    glm::vec2 mouse = glm::vec2(x_pos, y_pos);
    ubo->UBOSubBuffer(glm::value_ptr(mouse), sizeof(glm::vec2), sizeof(glm::vec2));
}

void scrollCallback(GLFWwindow* window, double x_offset, double y_offset) { }

static void updateShader(const int width, const int height)
{
    float i_time = glfwGetTime();
    glm::vec2 resolution = glm::vec2(width, height);

    ubo->UBOSubBuffer(glm::value_ptr(resolution), 0, sizeof(glm::vec2));
    ubo->UBOSubBuffer(&i_time, 2 * sizeof(glm::vec2), sizeof(float));
}
