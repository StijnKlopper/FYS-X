# FYS-X
Repository for a game made by Team FYS-X as the project during the thematic semester for Software Engineering.

_Made By_<br>
Tarik Yıldırım <br>
Stijn Klopper <br>
Rick den Otter <br>
Stan van Weringh <br>

## Best practices
Under this header, the best practices to ensure good teamwork will be explained.

### GIT/GitHub
There are three main branches:

- master, to keep the main code, updated the least often and code changes must be approved by all other team members.
- dev, to develop on, for every change made to the project.
- experiment, to do experimental stuff on, branch rules apply more loosely and most stuff developed here will likely not make it into the final product.

#### Developing new functionality
New functionality will start on the dev branch. The programmer will create a branch using the following naming scheme:
```
dev/{new/doc/fix/misc}/issue

Example for a pull request that changes the README.md:
dev/doc/update-readme

Example for a pull request that fixes a bug about chunks not generating:
dev/fix/chunks-not-generating

Example for a pull request that adds new functionality, adding biomes:
dev/new/biome-generation
```