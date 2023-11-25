#include "tests.hpp"
#include "init.hpp"

using namespace std;

const int _WIDTH = 800;
const int _HEIGHT = 600;

int main(int argc, char *argv[], char *envp[])
{
    std::string _folder = (argc > 1) ? std::string(argv[1]) : "./glsl/default/";

    initOpenGL(_WIDTH, _HEIGHT);
    run(_folder, _WIDTH, _HEIGHT);
}
