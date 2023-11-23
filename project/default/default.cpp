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

#define EX_MARCHING 02

static void processInput(GLFWwindow *, float);

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
    #endif // EX_MARCHING

    ubo = new UBO("General", 2 * sizeof(int), 0);

    ubo->UBOSubBuffer(&width , 0, sizeof(int));
    ubo->UBOSubBuffer(&height, sizeof(int), 2 * sizeof(int));

    shader->use();
    shader->setUniformBlockBinding(ubo->getName(), ubo->getBinding());

    std::vector<Texture2D*> textures = {};

    Mesh *quad = new Mesh(quad_vertices, quad_indices, textures, VERTEX_TYPE::ATTRIB_PNT);

    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);

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

static void updateShader(const int width, const int height)
{
    ubo->UBOSubBuffer(&width, 0, sizeof(int));
    ubo->UBOSubBuffer(&height, 0, 2 * sizeof(int));
}
